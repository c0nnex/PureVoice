using System;
using PureVoice.DSP;

namespace PureVoice.DSP
{
    public class ButterworthLowPassFilter : IVoiceFilter
    {

        //filter fc = 2hz, fs = 10hz

        private const int LowPassOrder = 4;

        private float[] inputValueModifier;
        private float[] outputValueModifier;
        private float[] inputValue;
        private float[] outputValue;
        private int valuePosition;

        public ButterworthLowPassFilter() : base()
        {
            inputValueModifier = new float[LowPassOrder];
            inputValueModifier[0] = 0.098531160923927f;
            inputValueModifier[1] = 0.295593482771781f;
            inputValueModifier[2] = 0.295593482771781f;
            inputValueModifier[3] = 0.098531160923927f;

            outputValueModifier = new float[LowPassOrder];
            outputValueModifier[0] = 1.0f;
            outputValueModifier[1] = -0.577240524806303f;
            outputValueModifier[2] = 0.421787048689562f;
            outputValueModifier[3] = -0.0562972364918427f;
        }

        public override float Process(float inputValue)
        {
            if (this.inputValue == null && this.outputValue == null)
            {
                this.inputValue = new float[LowPassOrder];
                this.outputValue = new float[LowPassOrder];

                valuePosition = -1;

                for (int i = 0; i < LowPassOrder; i++)
                {
                    this.inputValue[i] = inputValue;
                    this.outputValue[i] = inputValue;
                }

                return inputValue;
            }
            else if (this.inputValue != null && this.outputValue != null)
            {
                valuePosition = IncrementLowOrderPosition(valuePosition);

                this.inputValue[valuePosition] = inputValue;
                this.outputValue[valuePosition] = 0;

                int j = valuePosition;

                for (int i = 0; i < LowPassOrder; i++)
                {
                    this.outputValue[valuePosition] += inputValueModifier[i] * this.inputValue[j] -
                        outputValueModifier[i] * this.outputValue[j];

                    j = DecrementLowOrderPosition(j);
                }

                return this.outputValue[valuePosition];
            }
            else
            {
                throw new Exception("Both inputValue and outputValue should either be null or not null.  This should never be thrown.");
            }
        }


        private int DecrementLowOrderPosition(int j)
        {
            if (--j < 0)
            {
                j += LowPassOrder;
            }
            return j;
        }

        private int IncrementLowOrderPosition(int position)
        {
            return ((position + 1) % LowPassOrder);
        }

    }
}
