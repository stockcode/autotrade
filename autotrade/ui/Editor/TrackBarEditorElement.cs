using System;
using System.Drawing;
using System.Windows.Forms;
using Telerik.WinControls;
using Telerik.WinControls.Enumerations;
using Telerik.WinControls.UI;

namespace autotrade.ui.Editor
{
    public class TrackBarEditorElement : RadTrackBarElement
    {
        private TrackBarEditor editor;

        public TrackBarEditorElement(TrackBarEditor editor)
        {
            this.CanFocus = true;
            this.editor = editor;
            this.Maximum = 100;
            this.TickStyle = TickStyles.Both;
            this.SmallTickFrequency = 5;
            this.LargeTickFrequency = 20;
            this.BodyElement.ScaleContainerElement.TopScaleElement.StretchVertically = true;
            this.BodyElement.ScaleContainerElement.BottomScaleElement.StretchVertically = true;
            this.StretchVertically = false;
        }

        protected override Type ThemeEffectiveType
        {
            get { return typeof (RadTrackBarElement); }
        }

        public event EventHandler TrackPositionChanged;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            var grid = this.ElementTree.Control as RadGridView;
            if (grid != null)
            {
                switch (e.KeyCode)
                {
                    case Keys.Escape:
                    case Keys.Enter:
                    case Keys.Up:
                    case Keys.Down:
                        grid.GridBehavior.ProcessKeyDown(e);
                        break;
                    case Keys.Left:
                        if (this.Value > this.Minimum)
                        {
                            this.Value--;
                        }
                        break;
                    case Keys.Right:
                        if (this.Value < this.Maximum)
                        {
                            this.Value++;
                        }
                        break;
                    case Keys.Home:
                        this.Value = this.Minimum;
                        break;
                    case Keys.End:
                        this.Value = this.Maximum;
                        break;
                }
            }
        }

        protected override SizeF MeasureOverride(SizeF availableSize)
        {
            int desiredHeight = 40;
            foreach (RadElement element in this.Children)
            {
                element.Measure(new SizeF(availableSize.Width, desiredHeight));
            }
            return new SizeF(1, desiredHeight);
        }

        protected override SizeF ArrangeOverride(SizeF finalSize)
        {
            SizeF size = finalSize;
            size.Width -= 40;
            return base.ArrangeOverride(size);
        }

        public override void OnValueChanged(EventArgs e)
        {
            base.OnValueChanged(e);
            if (this.Parent != null && TrackPositionChanged != null)
            {
                TrackPositionChanged(this, EventArgs.Empty);
            }
        }
    }
}