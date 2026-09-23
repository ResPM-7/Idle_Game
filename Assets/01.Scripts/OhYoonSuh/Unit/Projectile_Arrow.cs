using UnityEngine;

public class Projectile_Arrow : Projectile
{
    [SerializeField, Min(0f)] private float rotationSpeed = 720f;
    [SerializeField] private float angleOffset;

    protected override void OnSetup()
    {
        if (Target == null) return;

        Vector3 direction = Target.position - transform.position;
        if (direction.sqrMagnitude > 0.000001f)
        {
            transform.rotation = GetTargetRotation(direction);
        }
    }

    protected override void UpdateFlight(Vector3 direction)
    {
        Quaternion targetRotation = GetTargetRotation(direction);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private Quaternion GetTargetRotation(Vector3 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(0f, 0f, angle + angleOffset);
    }
}
