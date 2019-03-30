using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace MCR_EMApp
{
    class FetchData
    {
        private int _currentMeterType, _start, _end, _meterTypes,_scantime;
        private string _modelNo, _jsonData;
        private JArray _jarrayReader;
        private JObject _jObject;
        private List<ushort> _currentReadings = new List<ushort>();
        private AppDBContext _context;
        //private List<ushort> _previousReadings = new List<ushort>();
        private List<ushort[]> _memAddress;
        private SerialPort _comport;
        private List<string> _tags;
        private bool _isEnergy = false;
        public FetchData(string pathToConfiguration, SerialPort comport,bool isEnergy)
        {
            _jsonData = File.ReadAllText(pathToConfiguration);
            _jarrayReader = JArray.Parse(_jsonData);
            _jObject = JObject.Parse(_jarrayReader[0].ToString());
            _comport = comport;
            _isEnergy = isEnergy;
        }
        //Getdata data from settings.json file
        private void loadSerialPortDetails()
        {
            _comport.PortName = _jObject["SerialPort"]["PortName"].ToString();
            _comport.BaudRate = int.Parse(_jObject["SerialPort"]["BaudRate"].ToString());
            _scantime= int.Parse(_jObject["SerialPort"]["ScanTime"].ToString());
            _comport.ReadTimeout = _scantime;
            _comport.WriteTimeout =_scantime;
            int parity = int.Parse(_jObject["SerialPort"]["Parity"].ToString());
            if (parity == 2)
            {
                _comport.Parity = Parity.Even;
            }
            else if (parity == 1)
            {
                _comport.Parity = Parity.Odd;
            }
            else
            {
                _comport.Parity = Parity.None;
            }
        }
        private void loadNoOfDifferentMeterTypes()
        {
            _meterTypes = int.Parse(_jObject["MeterTypes"].ToString());
        }
        private string getDeviceModel()
        {
            return _jObject["MeterDetails"][_currentMeterType.ToString()]["Model"].ToString();

        }
        private void loadMeterRange()
        {
            _start = int.Parse(_jObject["MeterDetails"][_currentMeterType.ToString()]["SlaveIdStart"].ToString());
            _end = int.Parse(_jObject["MeterDetails"][_currentMeterType.ToString()]["SlaveIdEnd"].ToString());
        }

        private void loadMemAddress()
        {
            _memAddress = new List<ushort[]>();
            _tags = new List<string>();
            foreach (var token in _jObject["MeterDetails"][_currentMeterType.ToString()]["MemoryMap"].ToList<JToken>())
            {
                JProperty jProperty = token.ToObject<JProperty>();
                string propertyName = jProperty.Name;
                _tags.Add(propertyName);
                _memAddress.Add(JArray.Parse(jProperty.Value.ToString()).Select(s=>ushort.Parse(s.ToString())).ToArray());
            }
        }
        private Excel.Worksheet FindSheet(Excel.Workbook workbook, string sheet_name)
        {
            foreach (Excel.Worksheet sheet in workbook.Sheets)
            {
                if (sheet.Name == sheet_name) return sheet;
            }

            return null;
        }
        public void GetDataFromMeters(Form1 frm)
        {

            try
            {
                //Load tags
                //loadTags();
                //Load Serial port data
                loadSerialPortDetails();
                //Get No of type of meters
                loadNoOfDifferentMeterTypes();
                //create new excel application 
                Excel.Application excel_app = new Excel.ApplicationClass();
                excel_app.Visible = false;
                StreamWriter file = new StreamWriter("output.txt");
                string newfile = System.IO.Directory.GetCurrentDirectory()+"\\Template.xlsx";
                //File.Copy(".//Template.xlsx", newfile);
                Excel.Workbook workbook = excel_app.Workbooks.Open(newfile, Type.Missing, Type.Missing, Type.Missing, Type.Missing,Type.Missing, Type.Missing, Type.Missing, Type.Missing,       Type.Missing, Type.Missing, Type.Missing, Type.Missing,Type.Missing, Type.Missing);
                string sheet_name = "";
                if (_isEnergy)
                    sheet_name = "Energy";
                else
                    sheet_name = "All_data";
                Excel.Worksheet sheet = FindSheet(workbook, sheet_name);
                if (sheet == null)
                {
                    // Add the worksheet at the end.
                    throw FileNotFoundException();
                }
                if (_isEnergy)
                {
                    sheet.Cells[5, 1] = DateTime.Now.ToString("dd-MMM-yyyy");
                    sheet.Cells[5, 2] = DateTime.Now.ToString("HH:mm");
                }
                //Get Meter Details from different type of meters
                for (int i = 1; i <= _meterTypes; i++)
                {
                    _currentMeterType = i;
                    //Load device model
                    _modelNo = getDeviceModel();
                    //Load slave id ranges of the meter
                    loadMeterRange();
                    //Load Memory address of modbus registers for Ir,Iy,Ib,Vr,Vy,Vb,KWH,MVAR,MW
                    loadMemAddress();
                    //Load Meter Ratio of individual meter and fetch data from meters
                    for (int j = _start; j <= _end; j++)
                    {
                        //Load sheet
                        //Update UI regarding meter      
                        frm.lblMeterNo.Text = "Currently reading meter no "+j.ToString();
                        //Load meter Ratios
                       // LoadMeterRatios(j);
                        //Get data from meters
                        file.WriteLine("Meter ID: " + j.ToString());
                        file.WriteLine(System.Environment.NewLine);
                        int m = 0;
                        foreach (var address in _memAddress)
                        {
                            //Updating UI regarding Tag
                            frm.lblTag.Text = "Currently reading Tag " + _tags[m].ToString();
                            m++;
                            //Constructing request header 
                            Byte[] requestwithoutcrc = new Byte[6];
                            Byte[] requestwithcrc = new Byte[8];
                            //first byte is the id of the meter 
                            //in this scenario j variable holds meter id                            
                            requestwithoutcrc[0] = Convert.ToByte(j);
                            //second byte is the function code of modbus
                            //As we are trying to read input registers function code is 4
                            requestwithoutcrc[1] = 4;
                            //The consecutive two bytes stores the register start address
                            //address[0] holds the register start address in int we are converting this to 16bit binary using convert2bitarray function and this is further converted to two 8 bit integers
                            Byte[] registerstartaddress = convert2int8(convert2bitarray(address[0], 16), 2);
                            for ( int k=registerstartaddress.Length-1,l=0;k>=0 ;k--,l++ )
                            {
                                requestwithoutcrc[l+2] = registerstartaddress[k];
                            }
                            //The consecutive two bytes stores the registers length
                            //address[1] holds the value ie., no registers needed from starting address this is also converted to two 8 bit integers
                            //response length variable holds no of bytes the output response will be
                            int responselength = address[1];
                            Byte[] response = new byte[responselength*2+5];
                            Byte[] responsedata=new byte[responselength * 2 ];
                            Byte[] length = convert2int8(convert2bitarray(address[1], 16), 2);
                            for (int k = length.Length-1, l = 0; k >= 0; k--, l++)
                            {
                                requestwithoutcrc[l + 4] = length[k];
                            }
                            //Till this point we have constructed the modbus request modbus request as crc is little indian in modbus
                            //The loop will calculate crc and appends crc bytes in little endian mode
                            byte[] crcbytes = calculatecrc(requestwithoutcrc);
                            requestwithoutcrc.CopyTo(requestwithcrc, 0);
                            for (int k = 0;k<crcbytes.Length ; k++)
                            {
                                requestwithcrc[k + 6] = crcbytes[k];
                            }

                            _comport.Open();
                            _comport.DiscardInBuffer();                           
                            _comport.Write(requestwithcrc,0,requestwithcrc.Length);
                            Thread.Sleep(1000);
                            if (_comport.BytesToRead == response.Length)
                            {
                                _comport.Read(response, 0, response.Length);
                            }
                            _comport.Close();

                            file.Write("Request: ");
                            foreach (byte byterequestdata in requestwithcrc)
                            {
                                file.Write(Convert.ToInt16(byterequestdata).ToString() + " ");
                            }
                            file.WriteLine(System.Environment.NewLine);


                            file.Write("Response: ");
                            foreach (byte byteresponsedata in response)
                            {
                                file.Write(Convert.ToInt16(byteresponsedata).ToString() + " ");
                            }

                            file.WriteLine(System.Environment.NewLine);
                            Array.Copy(response, 3, responsedata, 0, 4);
                            Array.Reverse(responsedata);
                            frm.lblValue.Text=_tags[m-1].ToString()+" : "+ BitConverter.ToInt32(responsedata, 0).ToString();
                            if(_isEnergy)
                                sheet.Cells[5, address[2]+j] = BitConverter.ToInt32(responsedata, 0).ToString();
                            else
                                sheet.Cells[j+4, address[2]] = BitConverter.ToInt32(responsedata, 0).ToString();
                        }
 
                    }
                }
                file.Close();
                workbook.Close(true, newfile, Type.Missing);
                excel_app.Quit();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private Exception FileNotFoundException()
        {
            throw new NotImplementedException("Template dont have Energy or Alldata named worksheet.");
        }

        private Byte[] convert2int8(BitArray bitArray, int v)
        {
            Byte[] array = new Byte[v];            
            bitArray.CopyTo(array, 0);
            return array;
        }

        private Byte[] calculatecrc(byte[] data)
        {
            string tbOut = "";
            BitArray crc = convert2bitarray(65535, 16);
            BitArray constMultiple = convert2bitarray(40961, 16);
            foreach (byte by in data)
            {
                int j = 1;
                BitArray ba = convert2bitarray(Convert.ToInt16(by), 16);
                crc.Xor(ba);
                int intcrca = ConvertToInt16(crc);
                for (int i = 0; i < 8; i++)
                {
                    int intcrc = 0;
                    if (crc[0] == false)
                    {
                        intcrc = ConvertToInt16(crc);
                        intcrc = intcrc >> 1;
                        crc = convert2bitarray(intcrc, 16);
                    }
                    else
                    {
                        intcrc = ConvertToInt16(crc);
                        intcrc = intcrc >> 1;
                        crc = convert2bitarray(intcrc, 16);
                        crc = crc.Xor(constMultiple);
                    }
                    tbOut += j.ToString() + "st xor of byte results " + ConvertToInt16(crc).ToString("X") + System.Environment.NewLine;

                }
            }
            Byte[] crcdata = convert2int8(crc, 2);
            return crcdata;
        }

        private int ConvertToInt16(BitArray crc)
        {
            int[] array = new int[1];
            crc.CopyTo(array, 0);
            return array[0];
        }

        private BitArray convert2bitarray(int intcrc, int length)
        {
            string s = Convert.ToString(intcrc, 2);
            BitArray temp = new BitArray(length);
            int padding = length - s.Length;
            for (int i = s.Length - 1, j = 0; i >= 0; i--, j++)
            {
                if (s[j].ToString() == "1")
                    temp[i] = true;
                else
                    temp[i] = false;
            }
            for (int i = s.Length; i < s.Length + padding; i++)
            {
                temp[i] = false;
            }
            return temp;
        }
    }
}





