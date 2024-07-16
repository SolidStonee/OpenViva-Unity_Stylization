using System.Collections.Generic;
using UnityEngine;

namespace Viva.Util
{

    public static class FlagField
    {
        public static bool IsSet<T>(T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            return (flagsValue & flagValue) != 0;
        }

        public static void Set<T>(ref T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue | flagValue);
        }

        public static void Unset<T>(ref T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue & (~flagValue));
        }
    }

    public class Set<T>
    {

        public readonly List<T> objects = new List<T>();
        public int Count { get { return objects.Count; } }
        public void Add(T obj)
        {
            if (objects.Contains(obj))
            {
                return;
            }
            objects.Add(obj);
        }

        public void Add(T[] array, float length)
        {
            length = Mathf.Min(array.Length, length);
            for (int i = 0; i < length; i++)
            {
                Add(array[i]);
            }
        }

        public bool Contains(T obj)
        {
            return objects.Contains(obj);
        }

        public void Remove(T obj)
        {
            objects.Remove(obj);
        }
    }

    public class CountedSet<T>
    {

        private readonly List<int> objectCounts = new List<int>();
        private readonly List<T> objects = new List<T>();
        public int Count { get { return objects.Count; } }
        public void Add(T obj)
        {
            int index = objects.IndexOf(obj);
            if (index != -1)
            {
                objectCounts[index] += 1;
                return;
            }
            objects.Add(obj);
            objectCounts.Add(1);
        }

        public T this[int index]
        {
            get { return objects[index]; }
        }

        public bool Contains(T obj)
        {
            return objects.Contains(obj);
        }

        public void Remove(T obj)
        {

            int index = objects.IndexOf(obj);
            if (index != -1)
            {
                if (--objectCounts[index] == 0)
                {
                    objectCounts.RemoveAt(index);
                    objects.RemoveAt(index);
                }
            }
        }
    }

    public class TransformBlend
    {
        public bool cacheTransform;
        public bool localPosBlend;
        public bool localRotBlend;
        public Transform target;
        public Vector3 startPos;
        public Quaternion startRot;
        public readonly Tools.EaseBlend blend = new Tools.EaseBlend();

        public static void LocalBlend(Transform target, Vector3 pos, Quaternion rot, float blend)
        {

            Vector3 localPos = Vector3.LerpUnclamped(
                target.localPosition,
                pos,
                blend
            );
            Quaternion localRot = Quaternion.LerpUnclamped(
                target.localRotation,
                rot,
                blend
            );
            target.localPosition = localPos;
            target.localRotation = localRot;
        }

        public static void WorldBlend(Transform target, Vector3 pos, Quaternion rot, float blend)
        {

            Vector3 worldPos = Vector3.LerpUnclamped(
                target.localPosition,
                pos,
                blend
            );
            Quaternion worldRot = Quaternion.LerpUnclamped(
                target.localRotation,
                rot,
                blend
            );
            target.position = worldPos;
            target.rotation = worldRot;
        }

        public void SetTarget(bool _cacheTransform, Transform _target, bool _localPosBlend, bool _localRotBlend, float start, float end, float duration)
        {

            cacheTransform = _cacheTransform;
            target = _target;
            localPosBlend = _localPosBlend;
            localRotBlend = _localRotBlend;
            if (cacheTransform)
            {
                if (localPosBlend)
                {
                    startPos = target.localPosition;
                }
                else
                {
                    startPos = target.position;
                }
                if (localRotBlend)
                {
                    startRot = target.localRotation;
                }
                else
                {
                    startRot = target.rotation;
                }
            }
            blend.reset(start);
            blend.StartBlend(end, duration);
        }
        public void Blend(Vector3 pos, Quaternion rot)
        {

            if (target == null)
            {
                Debug.LogError("TRANSFORM BLEND TARGET IS NULL!");
                return;
            }
            blend.Update(Time.deltaTime);
            if (!cacheTransform)
            {
                startPos = target.localPosition;
                startRot = target.localRotation;
            }
            if (localPosBlend)
            {
                target.localPosition = Vector3.LerpUnclamped(startPos, pos, blend.value);
            }
            else
            {
                target.position = Vector3.LerpUnclamped(startPos, pos, blend.value);
            }
            if (localRotBlend)
            {
                target.localRotation = Quaternion.LerpUnclamped(startRot, rot, blend.value);
            }
            else
            {
                target.rotation = Quaternion.LerpUnclamped(startRot, rot, blend.value);
            }
        }
    }

    public class ShapeKey
    {
        public Vector3[] deltaVertices;
        public Vector3[] deltaNormals;
        public Vector3[] deltaTangents;

        public ShapeKey(int size)
        {

            deltaVertices = new Vector3[size];
            deltaNormals = new Vector3[size];
            deltaTangents = new Vector3[size];
        }
    }

    public sealed class WriteIfNull<T>
    {

        private T value;
        public override string ToString()
        {
            return System.Convert.ToString(value);
        }
        public T Value
        {
            get { return this.value; }
            set
            {
                if (this.value != null)
                {
                    throw new System.InvalidOperationException("Cannot write value again");
                }
                this.value = value;
            }
        }
        public T ValueOrDefault { get { return this.value; } }

        public static implicit operator T(WriteIfNull<T> value) { return value.Value; }
    }

    public class Tuple<T1, T2>
    {
        public T1 _1;
        public T2 _2;

        public Tuple(T1 T1_, T2 T2_)
        {
            _1 = T1_;
            _2 = T2_;
        }
    }

    public static partial class Tools
    {

        public static float GetSide(Vector3 p, Transform reference)
        {
            return reference.InverseTransformPoint(p).z > 0 ? 1 : -1;
        }

        //public static PlayerHandState GetPlayerHandStateFromString(string hand)
        //{
        //    switch (hand)
        //    {
        //        case "right":
        //            return GameDirector.player.rightPlayerHandState;
        //        case "left":
        //            return GameDirector.player.leftPlayerHandState;
        //        default:
        //            Debug.LogError("HandState is null");
        //            return null;
        //    }            
        //}

        //public static T NearestCharacter<T>(List<T> list, Vector3 target) where T : Character
        //{
        //    float leastSqDist = Mathf.Infinity;
        //    T closest = null;
        //    if (list != null)
        //    {
        //        foreach (var character in list)
        //        {
        //            if (character == null)
        //            {
        //                continue;
        //            }
        //            float sqDist = Vector3.SqrMagnitude(character.floorPos - target);
        //            if (sqDist < leastSqDist)
        //            {
        //                leastSqDist = sqDist;
        //                closest = character;
        //            }
        //        }
        //    }
        //    return closest;
        //}

        public static bool RayIntersectsRectTransform(RectTransform transform, Ray ray, out Vector3 worldPosition, out float distance)
        {
            var corners = new Vector3[4];
            transform.GetWorldCorners(corners);
            var plane = new Plane(corners[0], corners[1], corners[2]);

            float enter;
            if (plane.Raycast(ray, out enter))
            {
                var intersection = ray.GetPoint(enter);

                var bottomEdge = corners[3] - corners[0];
                var leftEdge = corners[1] - corners[0];
                var bottomDot = Vector3.Dot(intersection - corners[0], bottomEdge);
                var leftDot = Vector3.Dot(intersection - corners[0], leftEdge);

                // If the intersection is right of the left edge and above the bottom edge.
                if (leftDot >= 0 && bottomDot >= 0)
                {
                    var topEdge = corners[1] - corners[2];
                    var rightEdge = corners[3] - corners[2];
                    var topDot = Vector3.Dot(intersection - corners[2], topEdge);
                    var rightDot = Vector3.Dot(intersection - corners[2], rightEdge);

                    //If the intersection is left of the right edge, and below the top edge
                    if (topDot >= 0 && rightDot >= 0)
                    {
                        worldPosition = intersection;
                        distance = enter;
                        return true;
                    }
                }
            }
            worldPosition = Vector3.zero;
            distance = 0;
            return false;
        }

        public class EaseBlend
        {

            private float old = 0.0f;
            private float target = 0.0f;
            private float curr = 0.0f;
            private float timer = 0.0f;
            private float duration = 1.0f;

            public bool finished { get { return curr == target; } }

            public float value { get { return curr; } }

            public EaseBlend()
            {
            }
            public void Update(float timeDelta)
            {
                timer = Mathf.Min(timer + timeDelta, duration);
                curr = old + (target - old) * EaseInOutQuad(timer / duration);
            }
            public float getDuration()
            {
                return duration;
            }
            public void reset(float _target)
            {
                curr = _target;
                target = _target;
                old = target;
            }
            public void StartBlend(float _target, float _duration)
            {
                old = curr;
                target = _target;
                timer = 0.0f;
                duration = Mathf.Max(0.0001f, _duration);   //prevent division by zero in Update()
            }
            public float getTarget()
            {
                return target;
            }
        }


        public static T EnsureComponent<T>(GameObject parent) where T : Component
        {
            T result = parent.GetComponent<T>();
            if (result == null)
            {
                return parent.AddComponent<T>();
            }
            return result;
        }

        public static T DuplicateComponent<T>(T original, GameObject destination) where T : Component
        {

            System.Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy as T;
        }
        public static float FlatXZVectorToDegrees(Vector3 vec)
        {
            if (vec.x == 0.0f && vec.z == 0.0f)
            {
                return 0;
            }
            return Mathf.Atan2(vec.x, vec.z) * Mathf.Rad2Deg;
        }

        public static byte[] UTF8ToByteArray(string input)
        {
            return System.Text.Encoding.UTF8.GetBytes(input);
        }

        public static string ByteArrayToUTF8(byte[] input, int index, int count)
        {
            return System.Text.Encoding.UTF8.GetString(input, index, count);
        }

        public static byte[] StringToBase64ByteArray(string input)
        {
            return System.Convert.FromBase64String(input);
        }

        public static string Base64ByteArrayToString(byte[] input, int index, int count)
        {
            return System.Convert.ToBase64String(input, index, count);
        }

        public static string UTF8ByteArrayToString(byte[] input, int index, int count)
        {
            return System.Text.Encoding.UTF8.GetString(input, index, count);
        }

        public static float Bearing(Transform source, Vector3 point)
        {

            Vector3 toSource = source.transform.position - point;
            float newFacingYaw = Mathf.Atan2(-toSource.x, -toSource.z) * Mathf.Rad2Deg;
            return Mathf.DeltaAngle(source.eulerAngles.y, newFacingYaw);
        }

        public static float RemapClamped(float low, float high, float lowOut, float highOut, float val)
        {
            return lowOut + (highOut - lowOut) * Mathf.Clamp01((val - low) / (high - low));
        }

        public static float ColorDistance(Color32 c1, Color32 c2)
        {
            long rmean = ((long)c1.r + (long)c2.r) / 2;
            long r = (long)c1.r - (long)c2.r;
            long g = (long)c1.g - (long)c2.g;
            long b = (long)c1.b - (long)c2.b;
            return Mathf.Sqrt((((512 + rmean) * r * r) >> 8) + 4 * g * g + (((767 - rmean) * b * b) >> 8));
        }

        public static float Pitch(Transform source, Transform toSource)
        {

            Vector3 local = source.InverseTransformPoint(toSource.position);
            float dist = local.x * local.x + local.z * local.z;
            if (dist == 0.0f)
            {
                return 0.0f;
            }
            dist = Mathf.Sqrt(dist);
            return Mathf.Atan2(local.y, dist) * Mathf.Rad2Deg;
        }
        
        public static float Remap(float value, float iMin, float iMax, float oMin, float oMax)
        {
            value = Mathf.Clamp(value, iMin, iMax);
            float a = Mathf.InverseLerp(iMin, iMax, value);
            return Mathf.Lerp(oMin, oMax, a);
        }
        
        public static float RemapFrom01(float value, float oMin, float oMax)
        {
            value = Mathf.Clamp01(value);
            return oMin + (oMax - oMin) * value;
        }
        
        public static float RemapTo01(float value, float iMin, float iMax)
        {
            //ensure value is within the input range
            value = Mathf.Clamp(value, iMin, iMax);

            //calculate the remapped value in the [0, 1] range
            return (value - iMin) / (iMax - iMin);
        }

        public static float EaseInOutCubic(float t)
        {
            return t < 0.5 ? 4.0f * t * t * t : (t - 1.0f) * (2.0f * t - 2.0f) * (2.0f * t - 2.0f) + 1.0f;
        }
        public static float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2.0f * t * t : -1.0f + (4.0f - 2.0f * t) * t;
        }
        public static float EaseInQuad(float t)
        {
            return t * t;
        }
        public static float EaseOutQuad(float t)
        {
            return t * (2.0f - t);
        }

        public static float EaseIn(float x)
        {
            return x * x;
        }

        public static float EaseOut(float x)
        {
            return 1.0f - EaseIn(1.0f - x);
        }
        
        public static float EaseInOut(float x)
        {
            float a = EaseIn(x);
            float b = EaseOut(x);
            return Mathf.Lerp(a, b, x);
        }
        
        public static float Frac(float x)
        {
            return Mathf.Abs(x - (int)x);
        }
        
        public static float Random(float x, float seed = 0.546f)
        {
            return Frac(Mathf.Sin((x + 0.3804f) * seed) * 143758.5453f);
        }

        public static float Smoothstep(float a, float b, float t)
        {
            float x = EaseInOut(t);
            return RemapFrom01(x, a, b);
        }
        
        public static float GradientNoise(float x)
        {
            float f = Frac(x);
            float t = EaseInOut(f);

            float previousInclination = (Random(Mathf.Floor(x)) * 2f) - 1f;
            float previousPoint = previousInclination * f;

            float nextInclination = (Random(Mathf.Ceil(x)) * 2f) - 1f;
            float nextPoint = nextInclination * (f - 1f);
            return Mathf.Lerp(previousPoint, nextPoint, t);
        }

        public static float GradientNoiseLayered(float x, float frequency, int octaves, float lacunarity, float gain)
        {
            float v = 0;
            float amp = 1;
            for (int i = 0; i < octaves; i++)
            {
                v += GradientNoise(x * frequency + i * 0.16291f) * amp;
                x *= lacunarity;
                amp *= gain;
            }
            return v;
        }
        
        public static float GetDensityFromVisibilityDistance(float distance)
        {
            const float factor = 3.912023005f;
            return factor / distance;
        }

        public static void ClearRenderTexture(RenderTexture textureToClear)
        {
            RenderTexture activeTexture = RenderTexture.active;
            RenderTexture.active = textureToClear;
            GL.Clear(true, true, new Color(0.0f, 0.0f, 0.0f, 1.0f));
            RenderTexture.active = activeTexture;
        }

        public static Mesh CreateQuad(float width = 1f, float height = 1f)
        {
            Mesh mesh = new Mesh();

            float w = width * 0.5f;
            float h = height * 0.5f;

            Vector3[] verts = new Vector3[4] { new Vector3(w, -h, 0), new Vector3(-w, -h, 0), new Vector3(w, h, 0), new Vector3(-w, h, 0) };

            int[] tris = new int[6] { 0, 2, 1, 2, 3, 1 };

            Vector3[] normals = new Vector3[4] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward, };

            Vector2[] uvs = new Vector2[4] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1), };

            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.normals = normals;
            mesh.uv = uvs;

            return mesh;
        }

        public static Vector3 ClampWithinSphere(Vector3 point, Vector3 center, float radius)
        {
            Vector3 dir = point - center;
            float currR = dir.magnitude;
            float clampedR = Mathf.Min(radius, currR);
            return center + (dir / currR) * clampedR;
        }

        public static float PointToSegmentDistance(Vector3 lineA, Vector3 lineB, Vector3 point)
        {

            Vector3 dir = lineB - lineA;
            float l = dir.magnitude;
            if (l == 0.0f)
            {
                return 0.0f;
            }
            dir /= l;
            Vector3 v = point - lineA;
            float d = Vector3.Dot(v, dir);
            Vector3 closest = lineA + dir * Mathf.Clamp(d, 0.0f, l);
            return Vector3.Distance(closest, point);
        }

        public static float SqDistanceToLine(Vector3 linePoint, Vector3 lineDir, Vector3 point)
        {
            return Vector3.Cross(lineDir, point - linePoint).sqrMagnitude;
        }

        public static float PointOnRayRatio(Vector3 lineA, Vector3 lineB, Vector3 point)
        {
            Vector3 dir = lineB - lineA;
            float dirLength = dir.magnitude;
            if (dirLength == 0.0f)
            {
                return 0.0f;
            }
            Vector3 norm = dir / dirLength;
            return Vector3.Dot(norm, point - lineA) / dirLength;
        }
        public static T[] CombineArrays<T>(T[] a, T[] b)
        {
            T[] result = new T[a.Length + b.Length];
            System.Buffer.BlockCopy(a, 0, result, 0, a.Length);
            System.Buffer.BlockCopy(b, 0, result, a.Length, b.Length);
            return result;
        }
        public static Bounds CalculateCenterAndBoundingHeight(GameObject obj, float padding)
        {

            List<Component> colliders = new List<Component>();
            obj.transform.GetComponents(typeof(Collider), colliders);

            Bounds initBounds = (colliders[0] as Collider).bounds;
            Bounds bounds = new Bounds(initBounds.center, initBounds.size);
            for (int i = 1; i < colliders.Count; i++)
            {
                Collider collider = colliders[i] as Collider;
                bounds.Encapsulate(collider.bounds);
            }
            bounds.min -= Vector3.one * padding;
            bounds.max += Vector3.one * padding;
            return bounds;
        }
        public static float GetClampedRatio(float lo, float hi, float test)
        {
            if (lo > hi)
            {
                return 1.0f - Mathf.Clamp01((test - lo) / (hi - lo));
            }
            return Mathf.Clamp01((test - lo) / (hi - lo));
        }
        public static void DrawCross(Vector3 pos, Color color, float radius = 0.4f)
        {
            Debug.DrawLine(pos + new Vector3(radius, 0.0f, 0.0f), pos - new Vector3(radius, 0.0f, 0.0f), color, 4.0f);
            Debug.DrawLine(pos + new Vector3(0.0f, radius, 0.0f), pos - new Vector3(0.0f, radius, 0.0f), color, 4.0f);
            Debug.DrawLine(pos + new Vector3(0.0f, 0.0f, radius), pos - new Vector3(0.0f, 0.0f, radius), color, 4.0f);
        }

        public static void DrawDiagCross(Vector3 pos, Color color, float radius = 0.4f, float duration = 4.0f)
        {
            Debug.DrawLine(pos + new Vector3(radius, radius, -radius), pos + new Vector3(-radius, -radius, radius), color, duration);
            Debug.DrawLine(pos + new Vector3(-radius, radius, radius), pos + new Vector3(radius, -radius, -radius), color, duration);
            Debug.DrawLine(pos + new Vector3(-radius, radius, -radius), pos + new Vector3(radius, -radius, radius), color, duration);
            Debug.DrawLine(pos + new Vector3(-radius, -radius, -radius), pos + new Vector3(radius, radius, radius), color, duration);
        }

        public static Vector2? CircleCircleIntersection(Vector2 p0, Vector2 p1, float r0, float r1, bool positiveSolution)
        {
            float d = Vector3.Distance(p0, p1);
            if (d == 0) return null;
            if (d >= r0 + r1) return null;
            if (d <= Mathf.Abs(r0 - r1)) return null;

            var a = (r0 * r0 - r1 * r1 + d * d) / (2 * d);
            var h = Mathf.Sqrt(r0 * r0 - a * a);
            var x2 = p0.x + a * (p1.x - p0.x) / d;
            var y2 = p0.y + a * (p1.y - p0.y) / d;
            if (positiveSolution) return new Vector2(x2 - h * (p1.y - p0.y) / d, y2 + h * (p1.x - p0.x) / d);
            else return new Vector2(x2 + h * (p1.y - p0.y) / d, y2 - h * (p1.x - p0.x) / d);
        }

        public static void AverageQuaternion(ref Vector4 cumulative, float weight, Quaternion newRotation, Quaternion firstRotation)
        {
            if (!AreQuaternionsClose(newRotation, firstRotation))
            {
                newRotation = new Quaternion(-newRotation.x, -newRotation.y, -newRotation.z, -newRotation.w);
            }
            cumulative.x += newRotation.x * weight;
            cumulative.y += newRotation.y * weight;
            cumulative.z += newRotation.z * weight;
            cumulative.w += newRotation.w * weight;
        }

        public static bool AreQuaternionsClose(Quaternion q1, Quaternion q2)
        {
            return Quaternion.Dot(q1, q2) >= 0.0f;
        }

        public static Vector3 ClosestPointOnBoxColliderSurface(Vector3 point, BoxCollider box)
        {
            var closest = box.ClosestPoint(point);
            var local = box.transform.InverseTransformPoint(closest) - box.center;

            float halfX = (box.size.x * 0.5f);
            float halfY = (box.size.y * 0.5f);
            float halfZ = (box.size.z * 0.5f);
            if (local.x > halfX || local.x < -halfX ||
                local.y > halfY || local.y < -halfY ||
                local.z > halfZ || local.z < -halfZ)
            {
                return closest;
            }
            else
            {
                Vector3 deltaAbs = new Vector3(
                    halfX - Mathf.Abs(local.x),
                    halfY - Mathf.Abs(local.y),
                    halfZ - Mathf.Abs(local.z)
                );
                var largest = Mathf.Min(deltaAbs.x, Mathf.Min(deltaAbs.y, deltaAbs.z));
                if (largest == deltaAbs.x)
                {
                    local.x = halfX * Mathf.Sign(local.x);
                }
                else if (largest == deltaAbs.y)
                {
                    local.y = halfY * Mathf.Sign(local.y);
                }
                else
                {
                    local.z = halfZ * Mathf.Sign(local.z);
                }
                return box.transform.TransformPoint(local);
            }
        }

        public static bool PointInsideBoxCollider(Vector3 point, BoxCollider box)
        {
            point = box.transform.InverseTransformPoint(point) - box.center;

            float halfX = (box.size.x * 0.5f);
            float halfY = (box.size.y * 0.5f);
            float halfZ = (box.size.z * 0.5f);
            if (point.x < halfX && point.x > -halfX &&
               point.y < halfY && point.y > -halfY &&
               point.z < halfZ && point.z > -halfZ)
                return true;
            else
                return false;
        }

        public static void GizmoArrow(Vector3 a, Vector3 b, float shrinkPercent = 0.0f)
        {

            Vector3 diff = b - a;
            a += diff * shrinkPercent * 0.5f;
            b -= diff * shrinkPercent * 0.5f;

            Gizmos.DrawLine(a, b);
            Vector3 norm = (b - a).normalized;
            norm = Quaternion.Euler(0.0f, -30.0f, 0.0f) * norm;
            Gizmos.DrawLine(b, b - norm * 0.04f);
            norm = Quaternion.Euler(0.0f, 60.0f, 0.0f) * norm;
            Gizmos.DrawLine(b, b - norm * 0.04f);
        }

        public static void DrawArrow(Vector3 a, Vector3 b, float shrinkPercent = 0.0f)
        {

            Vector3 diff = b - a;
            a += diff * shrinkPercent * 0.5f;
            b -= diff * shrinkPercent * 0.5f;

            Debug.DrawLine(a, b, Color.green, 4.0f);
            Vector3 norm = (b - a).normalized;
            norm = Quaternion.Euler(0.0f, -30.0f, 0.0f) * norm;
            Debug.DrawLine(b, b - norm * 0.04f, Color.green, 4.0f);
            norm = Quaternion.Euler(0.0f, 60.0f, 0.0f) * norm;
            Debug.DrawLine(b, b - norm * 0.04f, Color.green, 4.0f);
        }

        public static int SafeFloorToInt(float f)
        {
            return Mathf.FloorToInt(f + 0.01f);
        }

        public static Vector3 FlatForward(Vector3 forward)
        {
            forward.y = 0.0001f;
            return forward.normalized;
        }

        public static float CircleArea(float r)
        {
            return Mathf.PI * r * r;
        }

        public static float ApproximateVolume(Collider collider)
        {
            var sc = collider as SphereCollider;
            if (sc)
            {
                var r = sc.radius * collider.transform.lossyScale.y;
                return (4f / 3f) * Mathf.PI * r * r * r;
            }
            var cc = collider as CapsuleCollider;
            if (cc)
            {
                var r = cc.radius * collider.transform.lossyScale.y;
                var h = cc.height * collider.transform.lossyScale.y;
                var heightOnly = Mathf.Max(h, r * 2) / 2 - r;
                return Mathf.PI * r * r * ((4f / 3f) * r + heightOnly);
            }
            var bc = collider as BoxCollider;
            if (bc)
            {
                var size = Vector3.zero;
                size.x = collider.transform.lossyScale.x * bc.size.x;
                size.y = collider.transform.lossyScale.y * bc.size.y;
                size.z = collider.transform.lossyScale.z * bc.size.z;
                return size.x * size.y * size.z;
            }
            var mc = collider as MeshCollider;
            if (mc)
            {
                return collider.transform.lossyScale.x * mc.sharedMesh.bounds.size.magnitude;
            }
            return 0;
        }

        public static Vector3 DirToVector3(int coord)
        {
            switch (coord)
            {
                case 0:
                    return Vector3.right;
                case 1:
                    return Vector3.up;
                default:
                    return Vector3.forward;
            }
        }

        public static float CircleSectorArea(float r, float h)
        {
            if (r == 0) return 0;
            var H = Mathf.Clamp(Mathf.Abs(h), 0, r);
            float theta = Mathf.Acos(H / r) * 2;
            if (h < 0)
            {
                theta = 2 * Mathf.PI - theta;
            }
            return r * r * (theta - Mathf.Sin(theta)) / 2f;
        }

        public static Quaternion SafeLookRotation(Vector3 from, Vector3 to)
        {
            var diff = to - from;
            if (diff.sqrMagnitude <= Mathf.Epsilon) return Quaternion.identity;
            return Quaternion.LookRotation(diff, Vector3.up);
        }

        public static bool ExploreFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return false;
            }
            //Clean up file path so it can be navigated OK
            filePath = System.IO.Path.GetFullPath(filePath);
            System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", filePath));
            return true;
        }

        public static Texture2D CreateThumbnailTexture(string stringData, int resolution, TextureFormat format, bool mipMap)
        {
            var data = Tools.StringToBase64ByteArray(stringData);
            Texture2D thumbnailTex = new Texture2D(resolution, resolution, format, mipMap, true);
            try
            {
                thumbnailTex.LoadRawTextureData(data);
                thumbnailTex.Apply(false, true);
            }
            catch (System.Exception e)
            {
                Texture2D.DestroyImmediate(thumbnailTex);
                return null;
            }
            return thumbnailTex;
        }

        public static bool IsOverriden<T>(string methodName)
        {
            var m = typeof(T).GetMethod(methodName);
            if (m == null) return false;
            return m.GetBaseDefinition().DeclaringType != m.DeclaringType;
        }

        public static int CombineHashes(int a, int b)
        {
            int h = 17;
            h = h * 37 + a;
            return h * 37 + b;
        }

        public static Transform SearchTransformFamily(Transform branch, string target)
        {

            if (branch.name == target)
            {
                return branch;
            }
            Transform result = null;
            for (int i = 0; i < branch.childCount; i++)
            {
                Transform child = branch.GetChild(i);
                result = SearchTransformFamily(child, target);
                if (result != null)
                {
                    break;
                }
            }
            return result;
        }

        public static T SearchTransformAncestors<T>(Transform branch) where T : Component
        {
            //percolate down transforms until a Certain component is found
            Transform parent = branch;
            while (parent != null)
            {
                Component[] components = parent.GetComponents(typeof(Component));
                for (int i = 0; i < components.Length; i++)
                {
                    T candidate = components[i] as T;
                    if (candidate != null)
                    {
                        return candidate;
                    }
                }
                parent = parent.parent;
            }
            return null;
        }

        //returns true if an object that can be highlighted was found
        public static T FindClosestToSphere<T>(Vector3 point, float radius, int mask, QueryTriggerInteraction queryTrigger = QueryTriggerInteraction.Collide) where T : Component
        {

            Collider[] results = Physics.OverlapSphere(point, radius, mask, queryTrigger);
            if (results == null)
            {
                return null;
            }
            foreach (Collider result in results)
            {
                T candidate = SearchTransformAncestors<T>(result.transform);
                if (candidate == null)
                {
                    continue;
                }
                return candidate;
            }
            return null;
        }
    }
}