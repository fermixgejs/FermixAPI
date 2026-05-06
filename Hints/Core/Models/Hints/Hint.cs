namespace FermixAPI.Hints.Core.Models.Hints
{
    using FermixAPI.Hints.Core.Enum;

    /// <summary>
    /// Represents a hint displayed at a fixed position on the player's screen.
    /// </summary>
    public class Hint : AbstractHint
    {
        private HintAlignment alignment = HintAlignment.Center;
        private HintVerticalAlign yCoordinateAlign = HintVerticalAlign.Middle;

        private float xCoordinate = 0;
        private float yCoordinate = 700;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Hint"/> class with default values.
        /// </summary>
        public Hint()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Hint"/> class by copying properties from an existing hint.
        /// </summary>
        /// <param name="hint">The hint whose properties are copied into this instance.</param>
        public Hint(Hint hint)
            : base(hint)
        {
            Lock.EnterWriteLock();
            try
            {
                yCoordinate = hint.yCoordinate;
                xCoordinate = hint.xCoordinate;
                alignment = hint.alignment;
                yCoordinateAlign = hint.yCoordinateAlign;
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        internal Hint(DynamicHint hint, float x, float y)
            : base(hint)
        {
            Lock.EnterWriteLock();
            try
            {
                yCoordinate = y;
                xCoordinate = x;
                alignment = HintAlignment.Center;
                yCoordinateAlign = HintVerticalAlign.Bottom;
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }
        #endregion

        /// <summary>
        /// Gets or sets the Y coordinate of the hint. Higher Y coordinate means lower position
        /// Select from 0 to 1080 on any screen.
        /// </summary>
        public float YCoordinate
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return yCoordinate;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }

            set
            {
                Lock.EnterWriteLock();
                try
                {
                    if (yCoordinate.Equals(value))
                        return;

                    yCoordinate = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(YCoordinate));
            }
        }

        /// <summary>
        /// Gets or sets the horizontal offset of the hint. Higher X coordinate means more to the right
        /// This value should be between -1200 to 1200 including text length.
        /// </summary>
        public float XCoordinate
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return xCoordinate;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }

            set
            {
                Lock.EnterWriteLock();
                try
                {
                    if (xCoordinate.Equals(value))
                        return;

                    xCoordinate = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(XCoordinate));
            }
        }

        /// <summary>
        /// Gets or sets alignment of the hint.
        /// </summary>
        public HintAlignment Alignment
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return alignment;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }

            set
            {
                Lock.EnterWriteLock();
                try
                {
                    if (alignment == value)
                        return;

                    alignment = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(Alignment));
            }
        }

        /// <summary>
        /// Gets or sets the vertical alignment reference point used when interpreting <see cref="YCoordinate"/>.
        /// </summary>
        public HintVerticalAlign YCoordinateAlign
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return yCoordinateAlign;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }

            set
            {
                Lock.EnterWriteLock();
                try
                {
                    if (yCoordinateAlign == value)
                        return;

                    yCoordinateAlign = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

                OnHintUpdated(nameof(YCoordinateAlign));
            }
        }

        /// <summary>
        /// Not thread safe. Should only be used in pool.
        /// </summary>
        /// <param name="dynamicHint">The dynamic hint to be transform.</param>
        /// <param name="x">The X Coordinate.</param>
        /// <param name="y">The Y Coordinate.</param>
        internal void Set(DynamicHint dynamicHint, float x, float y)
        {
            this.CopyFieldsFrom(dynamicHint);

            this.xCoordinate = x;
            this.yCoordinate = y;
            this.alignment = HintAlignment.Center;
            this.yCoordinateAlign = HintVerticalAlign.Bottom;
        }
    }
}