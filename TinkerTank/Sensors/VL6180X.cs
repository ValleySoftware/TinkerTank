using System;
using System.Collections.Generic;
using System.Text;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Foundation;
using Meadow;
using System.Threading.Tasks;
using System.Threading;

namespace TinkerTank.Sensors
{

    public class HistoryBufferMap
    {
        private HistoryBufferMap()
        {
            //Hidden on purpose
        }

        public HistoryBufferMap(
            int bufferIndex, 
            int bufferRangeHigh, 
            int bufferRangeLow, 
            int bufferALSValue)
        {
            BufferIndex = bufferIndex;
            BufferRangeHigh = bufferRangeHigh;                
            BufferRangeLow = bufferRangeLow;
            BufferALSValue = bufferALSValue;
        }

        public int BufferIndex { get; }
        public int BufferRangeHigh { get; }
        public int BufferRangeLow { get; set; }
        public int BufferALSValue { get; set; }
    }


    public class VL6180X : I2cPeripheral
    {

        private static double DEFAULT_ALS_LUX_RESOLUTION = 0.32;

        private List<HistoryBufferMap> HistoryBufferLocations = new List<HistoryBufferMap>() { };

        private static double ALS_TO_LUX_CONVERSION(double ALS_LUX_RESOLUTION, double RESULT_ALS_VAL, double Analog_Gain, double ALS_INTEGRATION_TIME)
        {
            double result = -1;

            result = ALS_LUX_RESOLUTION * (RESULT_ALS_VAL * Analog_Gain) * (100 * ALS_INTEGRATION_TIME);

            return result;            
        }

        public enum RegisterIdentification
        {
            MODEL_ID = 0x000,
            MODEL_REV_MAJOR = 0x001,
            MODEL_REV_MINOR = 0x002,
            MODULE_REV_MAJOR = 0x003,
            MODULE_REV_MINOR = 0x004,
            DATE_HI = 0x006,
            DATE_LO = 0x007,
            TIME = 0x008, //0X008 -> 0X009
        }

        public enum RegisterSystemSetup
        {
            MODE_GPIO0 = 0x010,
            MODE_GPIO1 = 0x011,
            HISTORY_CTRL = 0x012,
            INTERUPT_CONFIG_GPIO = 0x014,
            INTERUPT_CLEAR = 0x015,
            FRESH_OUT_OF_COMPTON = 0x016, //FRESH_OUT_OF_RESET
            GROUPED_PARAMETER_HOLD = 0x017
        }

        public enum RegisterRangeSetup
        {
            TART = 0x018,
            THRESH_LOW = 0x019,
            THRESH_HIGH = 0x01A,
            INTERMEASUREMENT_PERIOD = 0x01B,
            MAX_CONVERGENCE_TIME = 0x01C,
            CROSSTALK_COMPENSATION_RATE = 0x01E,
            CROSSTALK_VALID_HEIGHT = 0x021,
            EARLY_CONVERGENCE_ESTIMATE = 0x022,
            PART_TO_PART_RANGE_OFFSET = 0x024,
            RANGE_IGNORE_VALID_HEIGHT = 0x025,
            RANGE_IGNORE_THRESHOLD = 0x026,
            MAX_AMBIENT_LEVEL_MULT = 0x02C,
            RANGE_CHECL_ENABLES = 0x02D,
            VHV_RECALIBRATE = 0x02E,
            VHV_REPEAT_RATE = 0x031
        }

        public enum RegisterAlsSetup
        {
            START = 0x038,
            THRESH_HIGH = 0x03A,
            THRESH_LOW = 0x03C,
            INTERMEASUREMENT_PERIOD = 0x03E,
            ANALOGUE_GAIN = 0x03F,
            INTERGRATION_PERIOD = 0x040
        }

        public enum RegisterResult
        {
            RANGE_STATUS = 0x04D,
            ALS_STATUS = 0x04E,
            INTERRUPT_STATUS_GPIO = 0x04F,
            ALS_VAL = 0x050,
            HISTORY_BUFFER_X = 0x052, //0X052->0X060
            RANGE_VAL = 0x062,
            RANGE_RAW = 0x064,
            RANGE_RETURN_RATE = 0x066,
            RANGE_REFERENCE_RATE = 0x068,
            RANGE_RETURN_SIGNAL_COUNT = 0x06C,
            RANGE_REFERENCE_SIGNAL_COUNT = 0x070,
            RANGE_RETURN_AMB_COUNT = 0x074,
            RANGE_REFERENCE_AMB_COUNT = 0x078,
            RANGE_RETURN_CONV_TIME = 0x07C,
            RANGE_REFERENCE_CONV_TIME = 0x080
        }

        public enum RegisterOther
        {
            READOUT_AVERAGING_SAMPLE_PERIOD = 0x10A,
            FIRMWARE_BOOTUP = 0x119,
            FIRMWARE_RESULT_SCALER = 0x120,
            I2C_SUBORDINATE_DEVICE_ADDRESS = 0x212,
            INTERLEAVED_MODE_ENABLE = 0x2A3
        }

        public VL6180X(II2cBus bus, byte peripheralAddress, int readBufferSize = 8, int writeBufferSize = 8) :
            base(bus, peripheralAddress, readBufferSize, writeBufferSize)
        {
            //Everything passed in is done in the base contructor.
            //Previously it would be done here.             

            //https://cdn-learn.adafruit.com/assets/assets/000/037/608/original/VL6180X_datasheet.pdf
            HistoryBufferLocations.Clear();
            HistoryBufferLocations.Add(new HistoryBufferMap(0, 15, 14, 7));
            HistoryBufferLocations.Add(new HistoryBufferMap(1, 13, 12, 6));
            HistoryBufferLocations.Add(new HistoryBufferMap(2, 11, 10, 5));
            HistoryBufferLocations.Add(new HistoryBufferMap(3, 9, 8, 4));
            HistoryBufferLocations.Add(new HistoryBufferMap(4, 7, 6, 3));
            HistoryBufferLocations.Add(new HistoryBufferMap(5, 5, 4, 2));
            HistoryBufferLocations.Add(new HistoryBufferMap(6, 3, 2, 1));
            HistoryBufferLocations.Add(new HistoryBufferMap(7, 1, 0, 0));
        }

        /// <summary>
        /// Retrieve the basic register locations for a given history record index.
        /// </summary>
        /// <description>
        /// Up to 8 records are available (0 indexed) with 0 being the newest 
        /// Note; this doesn't guarantee that there is a record there, nor does it get the record.  Just where it's data *SHOULD* be.
        /// </description>
        /// <param name="RecordIndexToGet">
        /// Index of the desired history map. 
        /// </param>
        /// <returns>
        /// A HistoryBufferMap containing its register locations.
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException">
        /// Value must be between 0 and 7 inclusive
        /// </exception>
        public HistoryBufferMap GetHistoryBufferRecord(int RecordIndexToGet)
        {
            if (HistoryBufferLocations.Count >= RecordIndexToGet ||
                HistoryBufferLocations.Count < 0)
            {
                throw new IndexOutOfRangeException();
            }

            return HistoryBufferLocations[RecordIndexToGet];
        }   
    }
}