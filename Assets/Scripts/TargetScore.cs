using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Collider))]
public class TargetScore : MonoBehaviour
{
    [Header("Scoring")]
    public float maxScore = 100f;
    public float scoringRadius = 1.0f;                // can be auto-computed from collider
    [Range(0.1f, 10f)] public float falloffExponent = 1.0f;
    public bool useRingScores = false;
    public float[] ringScores;

    [Tooltip("If set, this transform is used as the scoring center (useful if pivot isn't center).")]
    public Transform centerTransform;

    [Header("Auto / behavior")]
    [Tooltip("When true, scoringRadius will be computed from the Collider on Awake/OnValidate.")]
    public bool autoComputeRadiusFromCollider = true;
    [Tooltip("If a collision has no contact points, fall back to using ClosestPoint (useful for weird physics cases).")]
    public bool fallbackToClosestPoint = true;

    [Header("Runtime visuals")]
    public bool showRuntimeRings = true;
    public int ringSegments = 64;
    public float ringLineWidth = 0.02f;
    public Material ringMaterial; // optional - if null a simple material will be created

    [Header("Events")]
    public UnityEvent<int> onScored;
    [Tooltip("Optional reference to a Scoreboard script in the scene (will be auto-found if left blank).")]
    public Scoreboard scoreboard; // optional: if present, we'll call scoreboard.AddScore(int)

    // internal
    private Collider _collider;
    private readonly List<GameObject> _ringObjects = new List<GameObject>();

    private Vector3 CenterWorld => centerTransform != null ? centerTransform.position : transform.position;

    #region Unity lifecycle
    private void Reset()
    {
        _collider = GetComponent<Collider>();
        autoComputeRadiusFromCollider = true;
    }

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        if (autoComputeRadiusFromCollider) ComputeRadiusFromCollider();
        if (scoreboard == null) scoreboard = FindObjectOfType<Scoreboard>();
        CreateOrUpdateRuntimeRings();
    }

    private void OnValidate()
    {
        _collider = GetComponent<Collider>();
        if (autoComputeRadiusFromCollider) ComputeRadiusFromCollider();
        // update rings in editor
        CreateOrUpdateRuntimeRings();
    }

    private void OnDestroy()
    {
        // cleanup runtime-created objects if any
        ClearRuntimeRings();
    }
    #endregion

    #region Hit handling
    private void OnCollisionEnter(Collision collision)
    {
        Vector3 hitPoint;
        if (collision.contactCount > 0)
        {
            hitPoint = collision.GetContact(0).point;
        }
        else if (fallbackToClosestPoint)
        {
            hitPoint = collision.collider.ClosestPoint(CenterWorld);
            Debug.LogWarning($"Collision had no contact points — using ClosestPoint fallback for {name}");
        }
        else
        {
            Debug.LogWarning($"Collision had no contact points and fallback disabled on {name}");
            return;
        }

        ProcessHit(hitPoint, collision.gameObject);
    }

    // support trigger bullets if user prefers trigger-based bullets
    private void OnTriggerEnter(Collider other)
    {
        // closest point on the other collider to our center
        Vector3 hitPoint = other.ClosestPoint(CenterWorld);
        ProcessHit(hitPoint, other.gameObject);
    }

    private void ProcessHit(Vector3 hitPoint, GameObject source)
    {
        float score = CalculateScore(hitPoint);
        int finalScore = Mathf.RoundToInt(score);

        Debug.Log($"{name} hit by {source.name} at {hitPoint} -> score {finalScore}");

        // Fire UnityEvent for inspector hookup
        onScored?.Invoke(finalScore);

        // Auto-apply to scoreboard if present
        if (scoreboard != null)
        {
            scoreboard.AddScore(finalScore);
        }
    }
    #endregion

    #region Scoring math (same logic, kept public)
    public float CalculateScore(Vector3 hitPoint)
    {
        float dist = Vector3.Distance(hitPoint, CenterWorld);
        if (dist > scoringRadius) return 0f;

        if (useRingScores && ringScores != null && ringScores.Length > 0)
        {
            float ringWidth = scoringRadius / ringScores.Length;
            int ringIndex = Mathf.FloorToInt(dist / ringWidth);
            ringIndex = Mathf.Clamp(ringIndex, 0, ringScores.Length - 1);
            return ringScores[ringIndex];
        }
        else
        {
            float t = dist / scoringRadius;
            float value = 1f - Mathf.Pow(t, falloffExponent);
            return Mathf.Clamp01(value) * maxScore;
        }
    }
    #endregion

    #region Collider helpers
    private void ComputeRadiusFromCollider()
    {
        if (_collider == null) _collider = GetComponent<Collider>();
        if (_collider == null)
        {
            Debug.LogWarning($"{name}: no collider to compute scoringRadius from.");
            return;
        }

        // SphereCollider -> use radius * max scale axis
        if (_collider is SphereCollider sc)
        {
            scoringRadius = sc.radius * MaxScaleAxis();
        }
        else if (_collider is BoxCollider bc)
        {
            Vector3 scaled = Vector3.Scale(bc.size, transform.lossyScale);
            scoringRadius = Mathf.Max(Mathf.Abs(scaled.x), Mathf.Abs(scaled.y), Mathf.Abs(scaled.z)) * 0.5f;
        }
        else
        {
            // fallback to world bounds extents
            scoringRadius = Mathf.Max(_collider.bounds.extents.x, _collider.bounds.extents.y, _collider.bounds.extents.z);
        }

        if (scoringRadius <= 0f) scoringRadius = 0.001f;
    }

    private float MaxScaleAxis()
    {
        Vector3 ls = transform.lossyScale;
        return Mathf.Max(Mathf.Abs(ls.x), Mathf.Abs(ls.y), Mathf.Abs(ls.z));
    }
    #endregion

    #region Runtime ring rendering (LineRenderer)
    private void CreateOrUpdateRuntimeRings()
    {
        ClearRuntimeRings();

        if (!showRuntimeRings || scoringRadius <= 0f) return;

        int ringCount = (useRingScores && ringScores != null && ringScores.Length > 0) ? ringScores.Length : 1;
        float ringWidth = useRingScores ? scoringRadius / ringScores.Length : scoringRadius;

        for (int i = 1; i <= ringCount; i++)
        {
            float r = useRingScores ? ringWidth * i : scoringRadius;
            CreateRingObject(r, i, ringCount);
        }
    }

    private void CreateRingObject(float radius, int index, int total)
    {
        GameObject go = new GameObject($"scoringRing_{index}");
        go.transform.SetParent(transform, false); // keep in local space of target
        go.transform.localPosition = centerTransform != null ? transform.InverseTransformPoint(centerTransform.localPosition) : Vector3.zero;
        go.transform.localRotation = Quaternion.identity;

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.loop = true;
        lr.useWorldSpace = false; // positions are local to the go (which is parented to the target)
        lr.positionCount = ringSegments;
        lr.widthMultiplier = ringLineWidth;

        // material
        if (ringMaterial != null) lr.material = ringMaterial;
        else lr.material = new Material(Shader.Find("Sprites/Default"));

        // color - if rings defined, show gradient from red (outer) -> green (inner)
        Color col = Color.cyan;
        if (useRingScores && ringScores != null && ringScores.Length == total)
            col = Color.Lerp(Color.red, Color.green, index / (float)total);

        lr.startColor = col;
        lr.endColor = col;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.generateLightingData = false;

        // generate circle in local plane: X=right, Y=up => positions lie in target local XY plane (flush with target face)
        var pts = new Vector3[ringSegments];
        for (int s = 0; s < ringSegments; s++)
        {
            float angle = (2f * Mathf.PI * s) / ringSegments;
            pts[s] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
        }

        lr.SetPositions(pts);

        _ringObjects.Add(go);
    }

    private void ClearRuntimeRings()
    {
        for (int i = _ringObjects.Count - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(_ringObjects[i]);
            else Destroy(_ringObjects[i]);
#else
            Destroy(_ringObjects[i]);
#endif
        }
        _ringObjects.Clear();
    }
    #endregion

    #region Editor gizmo (keeps previous behaviour but aligned with target)
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        // draw same circles as before, but aligned to target's local plane (right/up)
        if (!UnityEditor.Selection.activeGameObject || !gameObject) { }
#endif
        if (scoringRadius <= 0f) return;

        if (useRingScores && ringScores != null && ringScores.Length > 0)
        {
            float ringWidth = scoringRadius / ringScores.Length;
            for (int i = 1; i <= ringScores.Length; i++)
            {
                float r = ringWidth * i;
                DrawCircleGizmo(CenterWorld, r, Color.Lerp(Color.red, Color.green, i / (float)ringScores.Length));
            }
        }
        else
        {
            DrawCircleGizmo(CenterWorld, scoringRadius, Color.cyan);
        }
    }

    private void DrawCircleGizmo(Vector3 center, float radius, Color color, int segments = 64)
    {
        Gizmos.color = color;
        Vector3 right = transform.right;
        Vector3 up = transform.up;

        Vector3 prev = center + right * radius;
        float step = 2f * Mathf.PI / segments;
        for (int i = 1; i <= segments; i++)
        {
            float a = i * step;
            Vector3 next = center + (Mathf.Cos(a) * right + Mathf.Sin(a) * up) * radius;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
    #endregion
}
