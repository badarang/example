using System.Collections.Generic;
using UnityEngine;

namespace Moleio.Core
{
    public sealed class MoleBodyTrail : MonoBehaviour
    {
        [SerializeField] private Transform segmentPrefab;
        [SerializeField] private Transform segmentRoot;
        [SerializeField] private float segmentSpacing = 0.35f;
        [SerializeField] private float followSpeed = 16f;
        [SerializeField] private int initialSegments = 8;

        private readonly List<Transform> segments = new();
        private readonly List<Vector3> pathPoints = new();
        private int ownerId;

        public int SegmentCount => segments.Count;

        private void Start()
        {
            if (segmentRoot == null)
            {
                segmentRoot = transform;
            }

            pathPoints.Clear();
            pathPoints.Add(transform.position);
            RebuildInitialSegments();
        }

        private void LateUpdate()
        {
            if (segments.Count == 0)
            {
                return;
            }

            pathPoints.Insert(0, transform.position);
            float maxDistance = Mathf.Max(segmentSpacing * (segments.Count + 2), 0.1f);
            TrimPath(maxDistance);

            for (int i = 0; i < segments.Count; i++)
            {
                Vector3 target = SamplePath(segmentSpacing * (i + 1));
                Transform current = segments[i];
                current.position = Vector3.MoveTowards(current.position, target, followSpeed * Time.deltaTime);
            }
        }

        public void Grow(int amount)
        {
            int safeAmount = Mathf.Max(0, amount);
            for (int i = 0; i < safeAmount; i++)
            {
                AddSegment();
            }
        }

        public void SetOwner(int id)
        {
            ownerId = id;
            for (int i = 0; i < segments.Count; i++)
            {
                ApplyOwner(segments[i]);
            }
        }

        public void RebuildInitialSegments()
        {
            ClearSegments();
            for (int i = 0; i < initialSegments; i++)
            {
                AddSegment();
            }
        }

        public IReadOnlyList<Transform> GetSegments()
        {
            return segments;
        }

        public Vector3[] ConsumeSegmentPositions()
        {
            Vector3[] positions = new Vector3[segments.Count];
            for (int i = 0; i < segments.Count; i++)
            {
                positions[i] = segments[i].position;
            }
            ClearSegments();
            return positions;
        }

        public void ClearSegments()
        {
            for (int i = 0; i < segments.Count; i++)
            {
                if (segments[i] != null)
                {
                    Destroy(segments[i].gameObject);
                }
            }

            segments.Clear();
        }

        private void AddSegment()
        {
            if (segmentPrefab == null)
            {
                return;
            }

            Vector3 spawnPos;
            if (segments.Count == 0)
            {
                spawnPos = transform.position;
            }
            else
            {
                spawnPos = segments[^1].position;
            }

            Transform segment = Instantiate(segmentPrefab, spawnPos, Quaternion.identity, segmentRoot);
            ApplyOwner(segment);
            segments.Add(segment);
        }

        private void ApplyOwner(Transform segment)
        {
            if (segment == null)
            {
                return;
            }

            MoleBodySegment marker = segment.GetComponent<MoleBodySegment>();
            if (marker == null)
            {
                marker = segment.gameObject.AddComponent<MoleBodySegment>();
            }

            marker.OwnerId = ownerId;
            marker.IsHead = false;
        }

        private void TrimPath(float maxDistance)
        {
            float totalDistance = 0f;
            for (int i = 1; i < pathPoints.Count; i++)
            {
                totalDistance += Vector3.Distance(pathPoints[i - 1], pathPoints[i]);
                if (totalDistance > maxDistance)
                {
                    pathPoints.RemoveRange(i, pathPoints.Count - i);
                    return;
                }
            }
        }

        private Vector3 SamplePath(float distanceFromHead)
        {
            if (pathPoints.Count == 0)
            {
                return transform.position;
            }

            float walked = 0f;
            for (int i = 1; i < pathPoints.Count; i++)
            {
                Vector3 a = pathPoints[i - 1];
                Vector3 b = pathPoints[i];
                float segmentDistance = Vector3.Distance(a, b);
                if (walked + segmentDistance >= distanceFromHead)
                {
                    float t = segmentDistance <= Mathf.Epsilon ? 0f : (distanceFromHead - walked) / segmentDistance;
                    return Vector3.Lerp(a, b, t);
                }

                walked += segmentDistance;
            }

            return pathPoints[^1];
        }
    }
}
