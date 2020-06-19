using D3DLab.ECS;
using D3DLab.Toolkit.D3Objects;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace D3DLab.Toolkit.Math3D {
    public sealed class VisualOctreeOutOfBoxException : Exception {
        public VisualOctreeOutOfBoxException() : base("Item is out off bounds.") {
        }
    }

    public class VisualOctree<T> {
        readonly OctreeNode<T> root;
        readonly Dictionary<T, OctreeItem<T>> items;
        readonly List<VisualPolylineObject> drawedDebug;
        public AxisAlignedBox Bounds { get { return root.Bounds; } }

        public VisualOctree(AxisAlignedBox box, int MaximumChildren) {
            root = OctreeNode<T>.CreateRoot(ref box, MaximumChildren);
            items = new Dictionary<T, OctreeItem<T>>();
            drawedDebug = new List<VisualPolylineObject>();
        }

        public bool Add(AxisAlignedBox box, T item) {
            var oitem = new OctreeItem<T>(ref box, item);

            if (root.Add(ref box, oitem)) {
                items.Add(item, oitem);
                return true;
            }
            return false;
        }
        public void Remove(T item) {
            items[item].SelfRemove();
            items.Remove(item);
        }

        public bool TryRemove(T item) {
            if (items.ContainsKey(item)) {
                Remove(item);
                return true;
            }
            return false;
        }

        public IEnumerable<OctreeItem<T>> GetColliding(Ray ray, Func<T, bool> predicate) {
            var result = new HashSet<OctreeItem<T>>();
            root.GetColliding(result, ref ray, float.PositiveInfinity, predicate);
            return result;
        }

        public IEnumerable<OctreeItem<T>> GetColliding(AxisAlignedBox box, Func<T, bool> predicate) {
            var result = new HashSet<OctreeItem<T>>();
            root.GetColliding(result, ref box, predicate);
            return result;
        }
        public bool HasCollision(AxisAlignedBox box, Func<T, bool> predicate) {
            var result = new HashSet<OctreeItem<T>>();
            root.GetColliding(result, ref box, predicate);
            return result.Any();
        }


        public void Clear() {
            foreach (var i in items) {
                i.Value.SelfRemove();
            }
            items.Clear();
            root.Clear();
        }


        public void Draw(IContextState context) {
            ClearDrew(context);            
            root.Draw(context, drawedDebug);
        }
        public void ClearDrew(IContextState context) {
            foreach (var b in drawedDebug) {
                b.Cleanup(context.GetEntityManager());
            }
            drawedDebug.Clear();
        }
    }

    public class OctreeNode<T> {
        private readonly Guid key;
        public static OctreeNode<T> CreateRoot(ref AxisAlignedBox box, int maximumChildren) {
            var node = new OctreeNode<T>(ref box, maximumChildren, null);
            return node;
        }

        const int NumChildNodes = 8;

        public AxisAlignedBox Bounds { get; }
        public OctreeNode<T> Parent { get; }
        public int MaximumChildren { get; }
        public OctreeNode<T>[] Nodes { get { return octants; } }

        OctreeNode<T>[] octants;
        readonly HashSet<OctreeItem<T>> items;

        public bool IsLeaf() { return !octants.Any(); }
        public bool IsEmpty() { return !items.Any(); }

        public bool IsRoot => Parent == null;

        public OctreeNode(ref AxisAlignedBox box, int maximumChildren, OctreeNode<T> parent) {
            key = Guid.NewGuid();
            Bounds = box;
            octants = Array.Empty<OctreeNode<T>>();
            MaximumChildren = maximumChildren;
            items = new HashSet<OctreeItem<T>>();
            Parent = parent;
        }


        OctreeNode<T>[] BuildNodes() {
            Vector3 dimensions = Bounds.Maximum - Bounds.Minimum;
            Vector3 half = dimensions * 0.25f;
            Vector3 center = Bounds.Center;
            var m_region = Bounds;
            var octant = new AxisAlignedBox[NumChildNodes];
            octant[0] = new AxisAlignedBox(m_region.Minimum, center);
            octant[1] = new AxisAlignedBox(new Vector3(center.X, m_region.Minimum.Y, m_region.Minimum.Z), new Vector3(m_region.Maximum.X, center.Y, center.Z));
            octant[2] = new AxisAlignedBox(new Vector3(center.X, m_region.Minimum.Y, center.Z), new Vector3(m_region.Maximum.X, center.Y, m_region.Maximum.Z));
            octant[3] = new AxisAlignedBox(new Vector3(m_region.Minimum.X, m_region.Minimum.Y, center.Z), new Vector3(center.X, center.Y, m_region.Maximum.Z));
            octant[4] = new AxisAlignedBox(new Vector3(m_region.Minimum.X, center.Y, m_region.Minimum.Z), new Vector3(center.X, m_region.Maximum.Y, center.Z));
            octant[5] = new AxisAlignedBox(new Vector3(center.X, center.Y, m_region.Minimum.Z), new Vector3(m_region.Maximum.X, m_region.Maximum.Y, center.Z));
            octant[6] = new AxisAlignedBox(center, m_region.Maximum);
            octant[7] = new AxisAlignedBox(new Vector3(m_region.Minimum.X, center.Y, center.Z), new Vector3(center.X, m_region.Maximum.Y, m_region.Maximum.Z));
            return octant.Select(x => new OctreeNode<T>(ref x, MaximumChildren, this)).ToArray();
        }

        bool RebuildTree() {
            if (Bounds.Diagonal < 4) {
                // node is too small
                return false;
            }
            var sw = new Stopwatch();
            sw.Start();
            octants = BuildNodes();
            //var nodes = new HashSet<OctreeNode<T>>();
            //sorting existing items 
            var old = items.ToList();
            items.Clear();
            for (int i = 0; i < old.Count; i++) {
                old[i].RemoveOwner(this);
                AddIntoNodes(old[i]);
            }
            sw.Stop();
            // Log.Debug($"RebuildTree time: {sw.ElapsedMilliseconds} ms");
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AddIntoNodes(OctreeItem<T> item) {
            for (int j = 0; j < octants.Length; j++) {
                var node = octants[j];

                var cross = node.Bounds.Contains(item.Bound);

                if (cross == AlignedBoxContainmentType.Contains) {
                    node.Add(item);
                    return;
                }
                if (cross == AlignedBoxContainmentType.Intersects) {
                    node.Add(item);
                    continue;
                }
            }
            //items.Add(item);
        }

        void Add(OctreeItem<T> item) {
            var box = item.Bound;
            if (!Add(ref box, item)) {
                //if can't add to any small boxes will add in parent box
                items.Add(item);
            }
        }

        public bool Add(ref AxisAlignedBox box, OctreeItem<T> item) {
            if (this.Bounds.Contains(ref box) == AlignedBoxContainmentType.Disjoint) {
                return false;
            }

            if (items.Count >= MaximumChildren && IsLeaf()) {
                if (this.Bounds.Contains(item.Bound) == AlignedBoxContainmentType.Contains) {
                    //split nodes if can
                    items.Add(item);
                    return RebuildTree();
                }
                return false;
            }
            if (!IsLeaf()) {
                AddIntoNodes(item);
            } else {
                item.AddOwner(this);
                items.Add(item);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetColliding(HashSet<OctreeItem<T>> result, ref Ray ray, float maxDistance, Func<T, bool> predicate) {
            if (!Bounds.Intersects(ref ray, out float distance) || distance > maxDistance) {
                return;
            }

            foreach (var item in items) {
                if (item.Bound.Intersects(ref ray, out distance) && distance <= maxDistance && predicate(item.Item)) {
                    result.Add(item);
                }
            }

            for (var j = 0; j < octants.Length; j++) {
                octants[j].GetColliding(result, ref ray, maxDistance, predicate);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetColliding(HashSet<OctreeItem<T>> result, ref AxisAlignedBox box, Func<T, bool> predicate) {
            if (!Bounds.Intersects(ref box)) {
                return;
            }
            foreach (var item in items) {
                if (item.Bound.Intersects(ref box) && predicate(item.Item)) {
                    result.Add(item);
                }
            }

            for (var j = 0; j < octants.Length; j++) {
                octants[j].GetColliding(result, ref box, predicate);
            }
        }

        public void Draw(IContextState context, List<VisualPolylineObject> drawed) {
            if (IsLeaf()) {
                drawed.Add(VisualPolylineObject.CreateBox(context, ElementTag.New(),
                    Bounds, V4Colors.Yellow));
            } else {
                for (int i = 0; i < Nodes.Length; i++) {
                    Nodes[i].Draw(context, drawed);
                }
            }
            foreach (var i in items) {
                drawed.Add(VisualPolylineObject.CreateBox(context, ElementTag.New("DEBUG_BOX_"),
                     i.Bound, V4Colors.Blue));
            }
        }

        public void Remove(OctreeItem<T> item) {
            items.Remove(item);
        }
        public void Clear() {
            foreach (var n in octants) {
                n.Clear();
            }
            octants = Array.Empty<OctreeNode<T>>();
            items.Clear();
        }
        public void Merge() {
            var sw = new Stopwatch();
            sw.Start();
            MergeUp(this);
            sw.Stop();
            //Log.Debug($"Merge time: {sw.ElapsedMilliseconds} ms");
        }
        static void MergeUp(OctreeNode<T> current) {
            while (!current.IsRoot) {
                var movedItems = new HashSet<OctreeItem<T>>();
                if (!current.CanMerged(movedItems) && !movedItems.Any()) {
                    return;
                }

                //movedItems.ForEach(x => current.items.Add(x));
                //movedItems.ForEach(x => x.AddOwner(current));
                //if (current.items.Count > current.MaximumChildren) {
                //    current.RebuildTree();
                //}
                current = current.Parent;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CanMerged(HashSet<OctreeItem<T>> movedItems) {
            var hasOctants = octants.Any();
            var hasItems = items.Any();
            if (!hasItems && !hasOctants) {
                return true;
            }

            if (hasOctants && octants.All(x => x.CanMerged(movedItems))) {
                //octants = null;
                octants = Array.Empty<OctreeNode<T>>();
            }
            //if (movedItems.Count >= MaximumChildren) {
            //    movedItems.ForEach(x => items.Add(x));
            //    movedItems.ForEach(x => x.AddOwner(this));
            //    movedItems.Clear();
            //    if (items.Count > MaximumChildren) {
            //        RebuildTree();
            //    }
            //    return false;
            //}
            //if (movedItems.Count < MaximumChildren && items.Count < MaximumChildren) {
            //    items.ForEach(x=>movedItems.Add(x));
            //    movedItems.ForEach(x => x.RemoveOwner(this));
            //    items.Clear();
            //    return true;
            //}

            return !hasItems;
        }
    }

    public class OctreeItem<T> {
        readonly List<OctreeNode<T>> owners;
        public AxisAlignedBox Bound { get; }
        public T Item { get; }
        public OctreeItem(ref AxisAlignedBox box, T item) {
            Item = item;
            Bound = box;
            owners = new List<OctreeNode<T>>();
        }
        public void AddOwner(OctreeNode<T> node) {
            owners.Add(node);
        }
        public void RemoveOwner(OctreeNode<T> node) {
            owners.Remove(node);
        }
        public void SelfRemove() {
            //var temp = owners.ToArray();
            owners.ForEach(x => x.Remove(this));
            owners.ForEach(x => x.Merge());
            owners.Clear();
        }
    }
}
