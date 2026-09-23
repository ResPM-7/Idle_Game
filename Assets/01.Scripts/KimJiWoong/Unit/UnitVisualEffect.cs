using UnityEngine;

public class UnitVisualEffect : MonoBehaviour
{
    [SerializeField] private Material damageBuffMaterial;
    [SerializeField] private Material freezeMaterial;

    private SpriteRenderer[] renderers;
    private Material[] defaultMaterials;

    private bool damageBuffActive;
    private bool frozenActive;

    private void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        defaultMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            defaultMaterials[i] = renderers[i].sharedMaterial;
        }
    }

    public void SetDamageBuff(bool active)
    {
        damageBuffActive = active;
        RefreshMaterials();
    }

    public void SetFrozen(bool active)
    {
        frozenActive = active;
        RefreshMaterials();
    }

    public void ResetEffects()
    {
        damageBuffActive = false;
        frozenActive = false;
        RefreshMaterials();
    }

    private void RefreshMaterials()
    {
        Material effectMaterial = null;

        // 동시에 걸리면 빙결 효과를 우선 표시
        if (frozenActive)
            effectMaterial = freezeMaterial;
        else if (damageBuffActive)
            effectMaterial = damageBuffMaterial;

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sharedMaterial =
                effectMaterial != null ? effectMaterial : defaultMaterials[i];
        }
    }

    private void OnDisable()
    {
        // 오브젝트 풀 재사용 시 효과가 남지 않게 초기화
        ResetEffects();
    }
}