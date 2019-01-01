using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemSoftware
{
    class Program
    {
        public class IO
        {
            public static List<List<string>> text = new List<List<string>>();
            public static string filePath = @"C:\Users\USER\Desktop\Fig2_5.txt";
            // 讀取文字檔，以字串形式傳回。
            public static List<string> fileToLine(string filePath)
            {
                if (!File.Exists(filePath))  
                    return null;
                var result = new List<string>();
                //每行資料
                var lines = File.ReadAllLines(filePath);//.Select(a => a.Split(' '))
                foreach (string str in lines)
                {
                    //1.將被'，'分割的字串聯起來，以此確認此行是否為空行 2.以"."開頭的為註解
                    if (str.Trim().Length!=0 && !str.StartsWith("."))
                        result.Add(str);
                }
                return result;
            }
            public static List<string> LineSplit(int linenumber,string line)
            {
                List<string> tokens = new List<string>();
                var result = line.Split('\t').ToList();
                for (int col = 0; col < result.Count; col++)
                { 
                    tokens.Add(result[col]);
                    if (col > 3)
                        Debug.log($"資料內容有誤:Line {linenumber} {line}");
                }
                if (result.Count < 3)
                    tokens.Add("");
                return tokens;
            }                                           
            public static void linetest(List<string> lines)
            {
                Console.WriteLine("-------ObjectProgram-------");
                foreach (var line in lines)
                {
                    Debug.log(line);
                }
            }
            public static string fileToText(string filePath)
            {
                if (!File.Exists(filePath))
                    return null;
                //吃每行資料
                var lines = File.ReadAllLines(filePath).Select(a => a.Split(' '));
                StreamReader file = new StreamReader(filePath);
                string text = file.ReadToEnd();
                file.Close();
                return text;
            }
        }
        class Debug
        {
            public static void error(String msg)
            {
                Debug.log(msg);
                //throw new Exception(msg);
            }

            public static void log(String msg)
            {
                Console.WriteLine(msg);
            }
        }
        public class ObjectProgram
        {
            private string Record = "";
            private string Project_startaddress = "";
            public string Output
            {
                get
                {
                    Debug.log("=============== ObjectProgram ==================");
                    return Record;
                }
            }
            public void HeaderRecord(string projectname,string startaddress,string endaddress)
            {
                Project_startaddress = startaddress;
                Record +=$"H{projectname}{startaddress.PadLeft(6, '0')}{ToSixHex(CalcHex(endaddress, startaddress))}" + Environment.NewLine;//H'專案名稱'起始位址'總長
            }
            public void TextRecord(string startaddress,string address)
            {
                string TotalLong = CalcTotalLong(address);
                Record += $"T{startaddress.PadLeft(6, '0')}{TotalLong.PadLeft(2, '0')}{address}" + Environment.NewLine;//T'起始位址'總長'位址'位址'位址...
            }
            public void EndRecord()
            {
                Record += $"E{Project_startaddress.PadLeft(6, '0')}";//E'起始位址
            }
            ///計算位數補足位數
            private string ToSixHex(string address)
            {
                return address.PadLeft(6, '0');//在不足六個字元的文字前面補零                
            }
            /// <summary>
            /// 十六進制相加(十六進制轉十進制加完後轉回十六進制)
            /// </summary>
            /// <param name="address1"></param>
            /// <param name="address2"></param>
            /// <returns></returns>
            private string CalcHex(string address1, string address2)
            {
                return Convert.ToString(Convert.ToInt32(address1, 16) - Convert.ToInt32(address2, 16), 16).ToUpper();
            }
            /// <summary>
            /// 計算Text一行總長度(利用string中字元長度計算)
            /// </summary>
            /// <param name="address">位址'位址'位址'位址'位址</param>
            /// <returns>(HEX表示)</returns>
            private string CalcTotalLong(string address)
            {
                return Convert.ToString(address.Length / 2); //每兩個字元等於1byte
            }
        }
        public class SicXeAssembler
        {
            public static string PSEUDO_OP = ",RESB,RESW,BYTE,WORD,START,END,BASE,";
            public static OpTable OPTAB = new OpTable();
            public SymbolTable SYMTAB = new SymbolTable();
            public ObjectProgram OBJ= new ObjectProgram();
            public static int LOCCTR = 0;
            private List<string> text;//每行輸入資料
            public List<string> Loc = new List<string>();
            public List<string> Objectcode = new List<string>();
            private Dictionary<string, int> Register = new Dictionary<string, int>()
            {
                { "A", 0 },{ "X", 1 },{ "L", 2 },{ "PC", 8 },{ "SW", 9 },{ "B", 3 },{ "S", 4 },{ "T", 5 },{ "F", 6 }
            };
            private static int PC, B = 0;
            public SicXeAssembler(List<string> Text)
            {
                text = Text;
            }
            public  List<string> LineSplit(int linenumber, string line)
            {
                List<string> tokens = new List<string>();
                var result = line.Split('\t').ToList();
                for (int col = 0; col < result.Count; col++)
                {
                    tokens.Add(result[col]);
                    if (col > 3)
                        Debug.log($"資料內容有誤:Line {linenumber} {line}");
                }
                if (result.Count < 3)
                    tokens.Add("");
                return tokens;
            }
            public int StartLOCCTR(List<string> firstline)// COPY     START   1000
            {
                int LOCCTR = 0;
                string programname = firstline[0];
                string op = firstline[1];

                if (op.Equals("START"))
                {
                    if (IsIllegalHexadecimal(firstline[2]))
                        Debug.log("Line 1 中所給的起始位址非十六進制'");
                    else
                        LOCCTR = Int32.Parse(firstline[2], System.Globalization.NumberStyles.AllowHexSpecifier);
                }                    
                else
                    Debug.log("Line 1 中沒有opcode 'START'");
                Loc.Add(TenToHex(LOCCTR, 4));
                return LOCCTR;
            }
            public IList<char> HexSet = new List<char>() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'a', 'b', 'c', 'd', 'e', 'f' };
            /// <summary>
            /// 判断十六进制字符串hex是否正确
            /// </summary>
            /// <param name="hex">十六进制字符串</param>
            /// <returns>true：不正确，false：正确</returns>
            public bool IsIllegalHexadecimal(string hex)
            {
                foreach (char item in hex)
                {
                    if (!HexSet.Contains<char>(item))
                        return true;
                }
                return false;
            }
            public void pass1()
            {
                Debug.log("=============== PASS1 ==================");
                LOCCTR = StartLOCCTR(LineSplit(0,text[0]));
                //int PC = LOCCTR;//下一行statement location 的值
                //判斷StartLOCCTR
                int num = 1;
                while (num!= text.Count)
                {
                    Loc.Add(TenToHex(LOCCTR, 4));
                    List<string> line = LineSplit(num, text[num]);
                    string symbol = line[0];
                    string op = line[1];
                    string operand = line[2];
                    string value;//"BYTE""WORD"的數值
                    if (symbol != "")//有symboltabel
                    {
                        if (SYMTAB.ContainsKey(symbol))
                            Debug.log($"{symbol} duplicate!");
                        else if (OPTAB.opToCode.ContainsKey(symbol))//LDT	LENGTH 避免對錯欄位的情況
                        {
                            op = line[0];
                            operand = line[1];
                        }
                        else
                            SYMTAB.Add(symbol, LOCCTR);
                    }
                    //處理特殊LOCCTR
                    bool isPseudo = (PSEUDO_OP.IndexOf("," + op + ",") >= 0);
                    if (isPseudo)
                    {
                        if (op.Equals("BYTE"))
                        {
                            if (operand.StartsWith("C'"))           // EOF      BYTE    C'EOF'
                            {
                                String str = operand.Substring(2, operand.Length - 3);
                                LOCCTR += str.Length;
                                value = "";
                                for (int si = 0; si < str.Length; si++)
                                {
                                    char ch = str[si];
                                    int chInt = ch;
                                    value += String.Format("{0:x2}", (uint)System.Convert.ToUInt32(chInt.ToString()));
                                }
                                value = value.ToUpper();
                            }
                            else if (operand.StartsWith("X'"))      // OUTPUT   BYTE    X'05'
                            {
                                String str = operand.Substring(2, operand.Length - 3);
                                value = str;
                                LOCCTR += str.Length/2;
                            }
                            else                                // THREE    BYTE    3
                                LOCCTR += 1;
                        }
                        else if (op.Equals("WORD"))             // FIVE     WORD    5
                        {
                            LOCCTR += 3;
                            value = String.Format("{0:X6}", Int32.Parse(operand));
                        }
                        else if (op.Equals("RESB"))             // BUFFER   RESB    1024
                            LOCCTR += Int32.Parse(Convert.ToString(int.Parse(operand), 16), System.Globalization.NumberStyles.HexNumber);
                        else if (op.Equals("RESW"))             // LENGTH   RESW    1
                            LOCCTR += int.Parse(operand) * 3;
                        else if (op.Equals("START"))            // COPY     START   1000
                            LOCCTR += int.Parse(operand);
                        else if (op.Equals("END"))
                            LOCCTR += 0;
                        else if (op.Equals("BASE"))
                            LOCCTR += 0;
                    }
                    else
                    {
                        if (OPTAB.opToCode.ContainsKey(op))
                        {
                            //string objCode = OPTAB.opToCode[op];
                            if (op.Equals("CLEAR") || op.Equals("TIXR")|| op.Equals("COMPR"))
                                LOCCTR += 2;
                            else
                                LOCCTR += 3;
                        }
                        else
                        {
                            if (op.StartsWith("+"))
                            {
                                LOCCTR += 4;
                                op = op.Substring(1, op.Length - 1);
                            }
                            else
                                Debug.error("op:" + op + " not found!");
                        }                                                     
                    }                                                     
                    num++;
                }
            }
            public void pass2()
            {
                Debug.log("=============== PASS2 ==================");
                Objectcode.Add("");//校正START不需要objectcode位置
                int num = 1;
                while (num != text.Count)
                {                    
                    List<string> line = LineSplit(num, text[num]);
                    string symbol = line[0];
                    string op = line[1];
                    string operand = line[2];
                    string value=null;//"BYTE""WORD"的數值
                    if (symbol != "")//有symboltabel
                    {
                        if (OPTAB.opToCode.ContainsKey(symbol))//LDT	LENGTH 避免對錯欄位的情況
                        {
                            op = line[0];
                            operand = line[1];
                        }
                    }
                    //處理特殊LOCCTR
                    bool isPseudo = (PSEUDO_OP.IndexOf("," + op + ",") >= 0);
                    if (isPseudo)
                    {
                        if (op.Equals("BYTE"))
                        {
                            if (operand.StartsWith("C'"))           // EOF      BYTE    C'EOF'
                            {
                                String str = operand.Substring(2, operand.Length - 3);
                                for (int si = 0; si < str.Length; si++)
                                {
                                    char ch = str[si];
                                    int chInt = ch;
                                    value += String.Format("{0:x2}", (uint)System.Convert.ToUInt32(chInt.ToString()));
                                }
                                value = value.ToUpper();
                            }
                            else if (operand.StartsWith("X'"))      // OUTPUT   BYTE    X'05'
                            {
                                String str = operand.Substring(2, operand.Length - 3);
                                value = str;
                            }
                            //else                                // THREE    BYTE    3
                            //    LOCCTR += 1;
                        }
                        else if (op.Equals("WORD"))             // FIVE     WORD    5
                        {
                            //LOCCTR += 3;
                            value = String.Format("{0:X6}", Int32.Parse(operand));
                        }
                        //else if (op.Equals("RESB"))             // BUFFER   RESB    1024
                        //    LOCCTR += Int32.Parse(Convert.ToString(int.Parse(operand), 16), System.Globalization.NumberStyles.HexNumber);
                        //else if (op.Equals("RESW"))             // LENGTH   RESW    1
                        //    LOCCTR += int.Parse(operand) * 3;
                        //else if (op.Equals("START"))            // COPY     START   1000
                        //    LOCCTR += Int32.Parse(Convert.ToString(int.Parse(operand), 16), System.Globalization.NumberStyles.HexNumber);
                        //else if (op.Equals("END"))
                        //    LOCCTR += 0;
                        else if (op.Equals("BASE"))
                            B = SYMTAB[$"{operand}"];
                        if (value!=null)
                            Objectcode.Add(value);
                        else
                            Objectcode.Add("");
                    }
                    else
                    {
                        PC = HexToTen(Loc[num+1]);
                        Objectcode.Add(ObjectCode(num,symbol, op, operand));
                    }
                    num++;
                }
                ObjectProgram();
            }
            public void ObjectProgram()//沒有OBJ.HeaderRecord
            {               
                List<string> Firstline = LineSplit(0, text[0]);
                OBJ.HeaderRecord(Firstline[0], Firstline[2], Convert.ToString(LOCCTR, 16));
                string textaddress = "";
                //FF換行 空行(RESW、RESB)換行
                for (int line = 0; line < Objectcode.Count; line++)
                {
                    if (Objectcode[line] == "" && textaddress != "")
                    {
                        OBJ.TextRecord(Loc[line], textaddress);
                        textaddress = "";
                    }
                    if (((textaddress.Length + Objectcode[line].Length) / 2) > 195)
                    {
                        OBJ.TextRecord(Loc[line], textaddress);
                        textaddress = "";
                    }
                    textaddress += Objectcode[line];
                }
                OBJ.EndRecord();
            }
            private string ObjectCode(int linenumber,string symbol, string op, string operand)
            {
                //需要知道PC B X 暫存器得值
                string nixbpe = "";//FlagBits
                string Format;
                bool isPseudo = (PSEUDO_OP.IndexOf("," + op + ",") >= 0);
                //if(!SYMTAB.ContainsKey(RemoveSymbol(operand)))
                //    Debug.log($"Operand有誤:Line {linenumber} Undefined symbol {operand}");
                if (op.Equals("CLEAR") || op.Equals("TIXR") || op.Equals("COMPR"))
                {
                    Format = OPTAB.opToCode[$"{RemoveSymbol(op)}"];//op 8 for fomat2
                    List<string> register = operand.Split(',').ToList();
                    for (int i = 0; i < 2; i++)//r1 r2 fomat2
                    {                       
                        foreach (var reg in Register)
                        {
                            if (register[i] == reg.Key)
                                Format += Convert.ToString(reg.Value, 16).ToUpper();
                        }
                        if (register.Count == 1)
                        {
                            Format += "0";
                            break;
                        }
                    }
                }
                else
                {
                    Format = HexToBinary(OPTAB.opToCode[$"{RemoveSymbol(op)}"]).PadLeft(8, '0').Substring(0, 6);//op 6 for fomat3.4
                    if (operand.StartsWith("@"))//set ni
                        nixbpe += "10";
                    else if (operand.StartsWith("#"))
                        nixbpe += "01";
                    else
                        nixbpe += "11";
                    if (operand.Contains(","))//set x
                        nixbpe += "1";
                    else
                        nixbpe += "0";

                    if (op.StartsWith("+"))//set e
                    {
                        nixbpe += "00";//set bp
                        nixbpe += "1";//set e format4
                        string address = null;//operand位址(symbolTable值)(5個字元 不足補零)
                        Format += nixbpe;
                        foreach (var sym in SYMTAB)
                        {
                            if (RemoveSymbol(operand) == sym.Key)
                                address = Convert.ToString(sym.Value, 16).ToUpper();
                        }
                        if (address != null)
                            Format = BinaryToHex(Format) + address.PadLeft(5, '0');
                        else if (operand.StartsWith("#"))//label
                            Format = BinaryToHex(Format) + TenToHex(int.Parse(RemoveSymbol(operand)), 5);
                        else
                            Debug.log($"SymbolTable中查無{symbol}的位址資訊!");
                    }
                    else
                    {
                        string disp = "";
                        //disp=(去除#)(3個字元 不足補零) => #operand本身(常數)
                        int constant;
                        if (int.TryParse(RemoveSymbol(operand), out constant))//operand為常數
                        {
                            disp = TenToHex(constant, 3);
                            nixbpe += "00";//set bp 
                        }
                        else if (op.Equals("RSUB"))
                        {
                            disp = "".PadLeft(3, '0');
                            nixbpe += "00";//set bp 
                        }
                        else
                        {
                            
                            //計算TA
                            string TA = "";//operand位址(symbolTable值) 
                            TA = Convert.ToString(SYMTAB[$"{RemoveSymbol(operand)}"], 16).PadLeft(4, '0').ToUpper();
                            //disp(十進位轉十六進位 3個字元 不足補零) = TA(十六進位轉十進位) - PC (若值為負數採二進位補數)
                            if ((HexToTen(TA) - PC) <= 2047 && (HexToTen(TA) - PC) >= -2048)//PC relative
                            {
                                disp = TenToHex(HexToTen(TA) - PC, 3);
                                nixbpe += "01";//set bp   
                            }
                            else
                            {
                                disp = TenToHex(HexToTen(TA) - B, 3);
                                nixbpe += "10";//set bp  
                                if ((HexToTen(TA) - B) > 4095 && (HexToTen(TA) - B) < 0)
                                    Debug.log("DeBug:編譯錯誤 未加+即使用format4"); 
                            }                                                        
                        }
                        nixbpe += "0";//set e format3
                        Format += nixbpe;
                        Format = BinaryToHex(Format).PadLeft(3, '0') + disp;
                    }
                }
                return Format;
            }
            private string RemoveSymbol(string text)
            {
                if(text.StartsWith("+")|| text.StartsWith("#")|| text.StartsWith("@"))
                    return text.Substring(1, text.Length - 1);
                if(text.Contains(","))
                    return text.Remove(text.IndexOf(","));
                return text;
            }
            public string TenToHex(int num,int length)//(若值為負數採二進位補數)
            {
                if(num<0)
                    return Convert.ToString(num, 16).Substring(8-length).ToUpper();//32
                return Convert.ToString(num, 16).PadLeft(length, '0').ToUpper();
            }
            private int HexToTen(string num)
            {
                return Convert.ToInt32(num, 16);
                //十六進位轉十進位
                //Int32.Parse(num, System.Globalization.NumberStyles.HexNumber);
            }
            private string HexToBinary(string hexString)
            {
                //十六進位轉十進位 十進位轉二進位
                return Convert.ToString(Convert.ToInt32(hexString, 16), 2);                
            }
            public string BinaryToHex(string BinaryString)
            {
                //二進位轉十進位 十進位轉十六進位            
                return Convert.ToString(Convert.ToInt32(BinaryString, 2), 16).ToUpper();
            }
        }
        

        public class OpTable
        {
            public Dictionary<String, String> opToCode = new Dictionary<String, String>();
            public Dictionary<String, String> codeToOp = new Dictionary<String, String>();
            public static String opCodes = "ADD=18,ADDF=58,ADDR=90,AND=40,CLEAR=B4,COMP=28,COMPF=88,COMPR=A0,DIV=24,DIVF=64,DIVR=9C,FIX=C4," +
                                    "FLOAT=C0,HIO=F4,J=3C,JEQ=30,JGT=34,JLT=38,JSUB=48,LDA=00,LDB=68,LDCH=50,LDF=70,LDL=08," +
                                    "LDS=6C,LDT=74,LDX=04,LPS=D0,MUL=20,MULF=60,MULR=98,NORM=C8,OR=44,RD=D8,RMO=AC,RSUB=4C," +
                                    "SHIFTL=A4,SHIFTR=A8,SIO=F0,SSK=EC,STA=0C,STB=78,STCH=54,STF=80,STI=D4,STL=14,STS=7C,STSW=E8," +
                                    "STT=84,STX=10,SUB=1C,SUBF=5C,SUBR=94,SVC=B0,TD=E0,TIO=F8,TIX=2C,TIXR=B8,WD=DC," +
                                    "RESB=,RESW=,BYTE=,WORD=,START=,END=";
            public OpTable()
            {
                Debug.log("================ OP TABLE ==================");
                String[] records = opCodes.Split(',');
                for (int i = 0; i < records.Length; i++)
                {
                    String[] tokens = records[i].Split('=');
                    opToCode.Add(tokens[0], tokens[1]);
                    if (tokens.Length > 1 && tokens[1].Length > 0)
                        codeToOp.Add(tokens[1], tokens[0]);
                    Debug.log(tokens[0] + "\t" + tokens[1]);
                }
            }
        }

        public class SymbolTable : Dictionary<string, int> {}
        static void Main(string[] args)
        {
            //輸入檔案位置
            Console.Write("請輸入檔案位置：");
            Console.WriteLine(IO.filePath);
            List<string> text = IO.fileToLine(IO.filePath);
            //List<string> text = IO.fileToLine(Console.ReadLine());//IO.filePath
            if (text==null)
                Console.WriteLine("檔案位置輸入錯誤，檔案不存在!");
            //IO.linetest(text);          
            SicXeAssembler asm = new SicXeAssembler(text);
            asm.pass1();
            foreach (var symbol in asm.SYMTAB)
            {
                Debug.log($"Symbol {symbol.Key.PadRight(8)}" + $"{Convert.ToString(symbol.Value, 16).PadLeft(4, '0').ToUpper()}");
            }
            asm.pass2();
            for (int line = 0; line < text.Count; line++)
            {
                List<string> col = asm.LineSplit(line, text[line]);
                if(col.Count<3)
                    Debug.log(asm.Loc[line] +" " + $"{"".PadRight(8)}" + $"{col[0].PadRight(8)}" + $"{col[1].PadRight(10)}" + $"{asm.Objectcode[line]}");
                else
                    Debug.log(asm.Loc[line] + " " + $"{col[0].PadRight(8)}" + $"{col[1].PadRight(8)}" + $"{col[2].PadRight(10)}" + $"{asm.Objectcode[line]}");
            }
            Debug.log(asm.OBJ.Output);
            Console.ReadKey();
        }
    }
}
