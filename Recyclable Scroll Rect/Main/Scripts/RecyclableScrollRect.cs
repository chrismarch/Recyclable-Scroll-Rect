//MIT License
//Copyright (c) 2020 Mohammed Iqubal Hussain
//Website : Polyandcode.com 

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PolyAndCode.UI
{
    /// <summary>
    /// Entry for the recycling system. Extends Unity's inbuilt ScrollRect.
    /// </summary>
    public class RecyclableScrollRect : ScrollRect
    {
        [HideInInspector]
        public IRecyclableScrollRectDataSource DataSource;

        public bool IsGrid;
        //Prototype cell can either be a prefab or present as a child to the content(will automatically be disabled in runtime)
        public RectTransform PrototypeCell;
        //If true the intiziation happens at Start. Controller must assign the datasource in Awake.
        //Set to false if self init is not required and use public init API.
        public bool SelfInitialize = true;

        public enum DirectionType
        {
            Vertical,
            Horizontal
        }

        public DirectionType Direction;

        public RectOffset Padding;
        public float Spacing;

        public Scrollbar VerticalScrollbar;

        //Segments : coloums for vertical and rows for horizontal.
        public int Segments
        {
            set
            {
                _segments = Math.Max(value, 2);
            }
            get
            {
                return _segments;
            }
        }
        [SerializeField]
        private int _segments;

        private RecyclingSystem _recyclingSystem;
        private Vector2 _prevAnchoredPos;
        private bool _initCoroutineRunning;
        
        protected override void Start()
        {
            //defafult(built-in) in scroll rect can have both directions enabled, Recyclable scroll rect can be scrolled in only one direction.
            //setting default as vertical, Initialize() will set this again. 
            vertical = true;
            horizontal = false;

            if (!Application.isPlaying) return;

            if (SelfInitialize) Initialize();
        }

        /// <summary>
        /// Initialization when selfInitalize is true. Assumes that data source is set in controller's Awake.
        /// </summary>
        private void Initialize(Action onInitialized = null)
        {
            if (!IsInitialized() && !_initCoroutineRunning)
            {
                //Contruct the recycling system.
                if (Direction == DirectionType.Vertical)
                {
                    _recyclingSystem = new VerticalRecyclingSystem(PrototypeCell, viewport, content, Padding, Spacing,
                                                                   DataSource, IsGrid, Segments);
                }
                else if (Direction == DirectionType.Horizontal)
                {
                    _recyclingSystem = new HorizontalRecyclingSystem(PrototypeCell, viewport, content, Padding, Spacing,
                                                                     DataSource, IsGrid, Segments);
                }

                vertical = Direction == DirectionType.Vertical;
                horizontal = Direction == DirectionType.Horizontal;

                _prevAnchoredPos = content.anchoredPosition;
                onValueChanged.RemoveListener(OnValueChangedListener);
                _initCoroutineRunning = true;
                //Adding listener after pool creation to avoid any unwanted recycling behaviour.(rare scenerio)
                StartCoroutine(_recyclingSystem.InitCoroutine(() =>
                                                              {
                                                                  onValueChanged.AddListener(OnValueChangedListener);
                                                                  if (onInitialized != null)
                                                                  {
                                                                      onInitialized();
                                                                  }
                                                                  _initCoroutineRunning = false;
                                                              }
                                                             ));
            }
        }

        /// <summary>
        /// public API for Initializing when datasource is not set in controller's Awake. Make sure selfInitalize is set to false. 
        /// </summary>
        public void Initialize(IRecyclableScrollRectDataSource dataSource, Action onInitialized = null)
        {
            DataSource = dataSource;
            Initialize(onInitialized);
        }

        /// <summary>
        /// Determine if this RecyclableScrollRect has started or finished initializing
        /// </summary>
        /// <returns>true if this RecyclableScrollRect has started or finished initializing</returns>
        public bool IsInitialized()
        {
            return _recyclingSystem != null;
        }

        /// <summary>
        /// Added as a listener to the OnValueChanged event of Scroll rect.
        /// Recycling entry point for recyling systems.
        /// </summary>
        /// <param name="direction">scroll direction</param>
        public void OnValueChangedListener(Vector2 normalizedPos)
        {
            Vector2 dir = content.anchoredPosition - _prevAnchoredPos;
            m_ContentStartPosition += _recyclingSystem.OnValueChangedListener(dir);
            _prevAnchoredPos = content.anchoredPosition;

            UpdateScrollbars();
        }

        /// <summary>
        /// Recycles all cells so they start at index. Use coroutine to wait if recycling system is initializing.
        /// </summary>
        public void JumpToCell(int index)
        {
            StartCoroutine(_recyclingSystem.RecycleToCell(index));
        }

        /// <summary>
        /// Calls SetCell on all current cells
        /// </summary>
        public void ResetCurrentCells()
        {
            _recyclingSystem.ResetCurrentCells();
        }

        /// <summary>
        ///Reloads the data. Call this if a new datasource is assigned.
        /// </summary>
        public void ReloadData(Action onDataReloaded = null)
        {
            ReloadData(DataSource, onDataReloaded);
        }

        /// <summary>
        /// Overloaded ReloadData with dataSource param
        ///Reloads the data. Call this if a new datasource is assigned.
        /// </summary>
        public void ReloadData(IRecyclableScrollRectDataSource dataSource, Action onDataReloaded = null)
        {
            if (IsInitialized() && this.gameObject.activeInHierarchy && !_initCoroutineRunning)
            {
                StopMovement();
                onValueChanged.RemoveListener(OnValueChangedListener);
                _recyclingSystem.DataSource = dataSource;
                _initCoroutineRunning = true;
                StartCoroutine(_recyclingSystem.InitCoroutine(() =>
                                                              {
                                                                  onValueChanged.AddListener(OnValueChangedListener);
                                                                  UpdateScrollbars();
                                                                  if (onDataReloaded != null)
                                                                  {
                                                                      onDataReloaded();
                                                                  }
                                                                  _initCoroutineRunning = false;
                                                              }
                                                             ));
                _prevAnchoredPos = content.anchoredPosition;
            }
        }

        private void UpdateScrollbars()
        {
            if (_recyclingSystem != null)
            {
                if (VerticalScrollbar != null)
                {
                    VerticalScrollbar.value = _recyclingSystem.CalcNormalizedScrollPosition();
                    VerticalScrollbar.size = _recyclingSystem.CalcNormalizedScrollbarSize();
                }
            }
        }
        
        #region  TESTING
        void OnDrawGizmos()
        {
            if (_recyclingSystem != null)
            {
                _recyclingSystem.OnDrawGizmos();
            }
        }
        #endregion
    }
}