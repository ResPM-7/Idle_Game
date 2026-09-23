using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>별도의 임시 씬에서 풀의 기본 동작을 검사합니다. 게임 씬을 열거나 저장 데이터를 읽지 않습니다.</summary>
public static class ObjectPoolSmokeTest
{
    [MenuItem("도구/검증/오브젝트 풀 기본 동작 검사")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("[오브젝트 풀 검사] 재생 모드를 중지한 뒤 검사를 실행해 주세요.");
            return;
        }
        Scene preview = EditorSceneManager.NewPreviewScene();
        try
        {
            var host = Create("PoolTest_Manager", preview);
            var manager = host.AddComponent<ObjectPoolManager>();
            var prefab = Create("PoolTest_Projectile", preview);
            prefab.AddComponent<Projectile>();
            var canvas = Create("PoolTest_Canvas", preview);
            var uiPrefab = Create("PoolTest_UI", preview);
            uiPrefab.AddComponent<RectTransform>();
            manager.objList.Add(new ObjectPoolManager.ObjectPoolItem
                { poolName = "Test", prefab = prefab, poolSize = 1 });
            manager.canvasPools.Add(new ObjectPoolManager.CanvasPoolItem
                { poolName = "UI", prefab = uiPrefab, targetCanvas = canvas.transform, poolSize = 1 });
            manager.Initialize();
            manager.Initialize(); // 반복 초기화해도 풀이 교체되거나 객체가 중복 보관되면 안 됩니다.

            var target = Create("PoolTest_Target", preview);
            GameObject first = manager.Spawn("Test", obj =>
            {
                Check(!obj.activeSelf, "초기화 함수가 비활성 상태에서 실행되는지 확인");
                obj.transform.position = new Vector3(2f, 3f, 0f);
                obj.GetComponent<Projectile>().Setup(target.transform, 10f, false, "Test");
            });
            Check(first != null && first.activeSelf && manager.IsRented(first), "생성 후 활성 상태와 사용 중 등록 확인");
            Check(first.transform.position == new Vector3(2f, 3f, 0f), "초기화에서 지정한 생성 위치 확인");
            manager.ReturnObject(first);
            Check(!first.activeSelf && !manager.IsRented(first), "반환 후 비활성 상태와 사용 종료 확인");
            var targetField = typeof(Projectile).GetField("target", BindingFlags.Instance | BindingFlags.NonPublic);
            Check(targetField.GetValue(first.GetComponent<Projectile>()) == null, "반환 콜백에서 투사체 타깃이 초기화되는지 확인");
            manager.ReturnObject(first); // 같은 객체를 두 번 반환해도 큐에는 한 번만 보관되어야 합니다.
            GameObject reused = manager.GetObject("Test");
            GameObject expanded = manager.GetObject("Test");
            Check(reused == first && expanded != first, "객체 재사용·풀 확장·중복 반환 방지 확인");
            manager.ReturnObject(reused);
            manager.ReturnObject(expanded);

            GameObject cancelled = manager.Spawn("Test", obj => manager.ReturnObject(obj));
            Check(cancelled == null, "초기화 중 반환한 객체가 활성화되지 않는지 확인");
            GameObject afterCancel = manager.GetObject("Test");
            Check(afterCancel != null, "초기화 취소 후에도 풀에서 다시 꺼낼 수 있는지 확인");
            manager.ReturnObject(afterCancel);

            for (int i = 0; i < 20; i++)
            {
                GameObject item = manager.Spawn("Test", obj =>
                    obj.GetComponent<Projectile>().Setup(target.transform, i, false, "Test"));
                Check(item != null, "반복 생성 확인");
                manager.ReturnObject(item);
                Check(targetField.GetValue(item.GetComponent<Projectile>()) == null, "반복 반환 시 타깃 초기화 확인");
            }

            GameObject ui = manager.GetObject("UI");
            Check(ui != null && ui.transform.parent.parent == canvas.transform, "캔버스 전용 풀의 부모 연결 확인");
            ui.transform.SetParent(target.transform, false);
            manager.ReturnObject(ui);
            Check(ui.transform.parent.parent == canvas.transform, "반환 시 원래 캔버스 부모로 복귀하는지 확인");
            Check(manager.GetObject("UI") == ui, "캔버스 객체 재사용 확인");
            manager.ReturnObject(ui);
            Debug.Log("[오브젝트 풀 검사 통과] 초기화·활성화·재사용·상태 정리·중복 반환 방지·생성 취소·풀 확장·캔버스 반환을 확인했습니다.");
        }
        finally
        {
            EditorSceneManager.ClosePreviewScene(preview);
        }
    }

    private static GameObject Create(string name, Scene scene)
    {
        var obj = new GameObject(name) { hideFlags = HideFlags.HideAndDontSave };
        SceneManager.MoveGameObjectToScene(obj, scene);
        return obj;
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("[오브젝트 풀 검사 실패] " + message);
    }
}
