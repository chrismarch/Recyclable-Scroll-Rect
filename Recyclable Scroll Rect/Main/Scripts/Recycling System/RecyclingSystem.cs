//MIT License
//Copyright (c) 2020 Mohammed Iqubal Hussain
//Website : Polyandcode.com 


using System.Collections;
using UnityEngine;
namespace PolyAndCode.UI
{
    /// <summary>
    /// Absract Class for creating a Recycling system.
    /// </summary>
    public abstract class RecyclingSystem
    {
        public IRecyclableScrollRectDataSource DataSource;

        protected RectTransform Viewport, Content;
        protected RectTransform PrototypeCell;
        protected RectOffset Padding;
        protected float Spacing;
        protected bool IsGrid;

        protected float MinPoolCoverage = 1.5f; // The recyclable pool must cover (viewPort * _poolCoverage) area.
        protected int MinPoolSize = 10; // Cell pool must have a min size
        protected float RecyclingThreshold = .2f; //Threshold for recycling above and below viewport

        protected Vector3[] _contentWorldCorners = new Vector3[4];
        protected Vector3[] _viewWorldCorners    = new Vector3[4];

        //Multiply ratio of viewport width (or height)/cell width (or height) by this to get correctly tuned
        //RecyclingThreshold for those dimensions. Original tuning appears to have been done with square cells,
        //at a lower resolution. This multiplier allows for non-square cells to recycle correctly, and regardless
        //of scaling, preventing visible pop-in, and stickiness with elasticity.
        protected const float RECYCLING_THRESHOLD_TUNING_MULTIPLIER = 0.2f * (111.566673f)/974.812439f;
        
        public abstract IEnumerator InitCoroutine(System.Action onInitialized = null);

        public abstract Vector2 OnValueChangedListener(Vector2 direction);

        public abstract IEnumerator RecycleToCell(int index);

        public abstract void  ResetCurrentCells();

#region scrollbars
        public abstract float CalcNormalizedScrollPosition();

        public abstract float CalcNormalizedScrollbarSize();

        protected abstract Bounds CalcVirtualContentBounds();
#endregion scrollbars

        protected static Bounds CalcBounds(Vector3[] corners)
        {
            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int j = 0; j < 4; j++)
            {
                Vector3 v = corners[j];
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            var bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
            return bounds;
        }

        #region  TESTING
        public virtual void OnDrawGizmos()
        {
        }
        #endregion
    }
}