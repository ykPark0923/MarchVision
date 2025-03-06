using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JidamVision.Property
{
    public partial class BinaryInspProp : UserControl
    {
        public event EventHandler<RangeChangedEventArgs> RangeChanged;


        /* NOTE
        public int LowerValue
        {
            get { return trackBarLower.Value; }
        }
        C# 6부터는 위 코드를 더 간결하게 람다(=>) 문법을 사용하여 표현
        */

        // 속성값을 이용하여 이진화 임계값 설정
        public int LowerValue => trackBarLower.Value;
        public int UpperValue => trackBarUpper.Value;

        public BinaryInspProp()
        {
            InitializeComponent();

            // TrackBar 초기 설정
            trackBarLower.ValueChanged += OnValueChanged;
            trackBarUpper.ValueChanged += OnValueChanged;

            trackBarLower.Value = 0;
            trackBarUpper.Value = 128;
        }

        private void OnValueChanged(object sender, EventArgs e)
        {
            RangeChanged?.Invoke(this, new RangeChangedEventArgs(LowerValue, UpperValue));
        }

    }
    public class RangeChangedEventArgs : EventArgs
    {
        public int LowerValue { get; }
        public int UpperValue { get; }

        public RangeChangedEventArgs(int lowerValue, int upperValue)
        {
            LowerValue = lowerValue;
            UpperValue = upperValue;
        }
    }
}
