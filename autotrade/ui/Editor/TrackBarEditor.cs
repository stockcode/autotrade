using System;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace autotrade.ui.Editor
{
    public class TrackBarEditor : BaseGridEditor
    {
        public override object Value
        {
            get { return TrackBarElement.Value; }
            set
            {
                if (value != null && value != DBNull.Value)
                {
                    TrackBarElement.Value = Convert.ToInt32(value);
                }
                else
                {
                    TrackBarElement.Value = 0;
                }
            }
        }

        public TrackBarEditorElement TrackBarElement
        {
            get { return EditorElement as TrackBarEditorElement; }
        }

        public int Minimum
        {
            get { return (int) TrackBarElement.Minimum; }
            set { TrackBarElement.Minimum = value; }
        }

        public int Maximum
        {
            get { return (int) TrackBarElement.Maximum; }
            set { TrackBarElement.Maximum = value; }
        }

        public int TickFrequency
        {
            get { return TrackBarElement.TickFrequency; }
            set { TrackBarElement.TickFrequency = value; }
        }

        public override Type DataType
        {
            get { return typeof (Int32); }
        }

        public override void Initialize(object owner, object value)
        {
            base.Initialize(owner, value);
            EditorElement.Focus();
            TrackBarElement.Value = (int) value;
        }

        public override void BeginEdit()
        {
            base.BeginEdit();
            ((GridCellElement) EditorElement.Parent).Text = Value + " %";
            ((TrackBarEditorElement) EditorElement).TrackPositionChanged +=
                new EventHandler(TrackBarEditor_TrackPositionChanged);
        }

        public override bool EndEdit()
        {
            ((TrackBarEditorElement) EditorElement).TrackPositionChanged -=
                new EventHandler(TrackBarEditor_TrackPositionChanged);
            return base.EndEdit();
        }

        private void TrackBarEditor_TrackPositionChanged(object sender, EventArgs e)
        {
            ((GridCellElement) EditorElement.Parent).Text = Value + " %";
            OnValueChanged();
        }

        protected override RadElement CreateEditorElement()
        {
            return new TrackBarEditorElement(this);
        }
    }
}