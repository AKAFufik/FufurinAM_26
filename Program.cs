using System;

namespace Assembler
{
    public class Program
    {
        static int[] memory = new int[256]; //4 байта
        static int[] cmemory = new int[256]; //4 байта
        static int[] registers = new int[8];
        static int pc = 0;


        public static void Main()
        {
            WriteMemory(
                new int[] {
                    0b00000000_00000000_00000000_00000000, //0 : Инициализация массива
                    0b00000000_00000000_00000000_00001000, //1 : Размер массива (8)
                    0b00000000_00000000_00000000_00000101, //2 : [0] = 5
                    0b00000000_00000000_00000000_00001000, //3 : [1] = 8
                    0b00000000_00000000_00000000_00001010, //4 : [2] = 10
                    0b00000000_00000000_00000000_00001100, //5 : [3] = 12
                   -0b00000000_00000000_00000000_00001001, //6 : [4] = -9
                    0b00000000_00000000_00000000_00001010, //7 : [5] = 10
                    0b00000000_00000000_00000000_00001011, //8 : [6] = 11
                   -0b00000000_00000000_00000000_00000011, //9 : [7] = 3
                },

                new string[] {
                          "MOV RI 2 2",
                          "MOV RI 3 1",
                          "MOV RX 4 1",
                          "MOV RD 5 2",

                          "MOV RR 0 5",
                          "MOV RR 1 2",
                          "ADD RR 1 3",
                          "MOV RD 1 1",
                          "CMP RR 0 1",
                          "JG RI 0 11",
                          "MOV RR 5 1",
                          "ADD RI 3 1",
                          "MOV RR 0 3",
                          "CMP RR 0 4",
                          "JNE RI 0 4",
                          "MOV RR 0 5",

                          "INT IR 1 0",
                          "INT IR 0 0"
                }
            );

            bool exitFlag = false;
            while (!exitFlag)
            {
                /*string str = cmemory[pc];
                string[] command = str.Split();

                byte commandCode = GetCommandBin(command[0]);
                byte commandFlags = (byte)(int.Parse(command[1]));
                byte op1 = (byte)(int.Parse(command[2]));
                byte op2 = (byte)(int.Parse(command[3]));*/

                int command = cmemory[pc];

                byte commandCode = (byte)((command >> 24) & 0xff); // AND 00000000000000000000000011111111
                byte commandFlags = (byte)((command >> 16) & 0xff);
                byte op1 = (byte)((command >> 8) & 0xff);
                byte op2 = (byte)(command & 0xff);

                PrintAsm(commandCode, commandFlags, op1, op2);

                switch (commandCode)
                {
                    case 0: //EMPTY
                        pc++;
                        break;
                    case 1: //MOV
                        {
                            // Получение типов операндов
                            byte op1Type = GetOperandType(commandFlags, 1); // куда записывать
                            byte op2Type = GetOperandType(commandFlags, 2); // откуда брать значения
                            // Получение значений второго операнда
                            int value = GetValue(op2, op2Type);
                            // Установка значений первого операнда
                            SetValue(op1, op1Type, value);
                            // Переход к следующей команде
                            pc++;
                        }
                        break;
                    case 2: //ADD
                        {
                            // Получение значений и типов операндов
                            byte op1Type = GetOperandType(commandFlags, 1); // куда записывать
                            int op1Value = GetValue(op1, op1Type);

                            byte op2Type = GetOperandType(commandFlags, 2); // откуда брать значения
                            int op2Value = GetValue(op2, op2Type);

                            // Установка в качестве значения первого операнда сумму  значений операндов
                            SetValue(op1, op1Type, op1Value + op2Value);
                            pc++;
                        }
                        break;
                    case 3: //CMP сравнивает значения операндов и помещает в ячейку операнда 1 результат сравнения
                        {
                            // Получение значения операндов
                            byte op1Type = GetOperandType(commandFlags, 1); // куда записывать
                            int op1Value = GetValue(op1, op1Type);
                            byte op2Type = GetOperandType(commandFlags, 2); // откуда брать значения
                            int op2Value = GetValue(op2, op2Type);
                            byte result;

                            // Получение результата в зависимости от значений операндов
                            if (op1Value > op2Value)
                            {
                                result = 0b00;
                            }
                            else if (op1Value < op2Value)
                            {
                                result = 0b10;
                            }
                            else
                            {
                                result = 0b01;
                            }
                            SetValue(op1, op1Type, result);
                            pc++;
                        }
                        break;
                    case 4: //JNE
                        {
                            // Получение значения операндов
                            byte op1Type = GetOperandType(commandFlags, 1); // куда записывать
                            int op1Value = GetValue(op1, op1Type);
                            byte op2Type = GetOperandType(commandFlags, 2); // откуда брать значения
                            int op2Value = GetValue(op2, op2Type);

                            // Если значение первого операнда не равен результату проверки "не равно", переход на команду под индексом, получаемым из второго операнда
                            if (op1Value != 0b01)
                            {
                                pc = op2Value;
                            }
                            else //В противном случае переход на следующую команду
                            {
                                pc++;
                            }
                        }
                        break;
                    case 5: //JG
                        {
                            byte op1Type = GetOperandType(commandFlags, 1); // куда записывать
                            int op1Value = GetValue(op1, op1Type);
                            byte op2Type = GetOperandType(commandFlags, 2); // откуда брать значения
                            int op2Value = GetValue(op2, op2Type);
                            // Аналогично команде JNE, но переход осуществляется при результате "больше" в первом операнде
                            if (op1Value == 0b00)
                            {
                                pc = op2Value;
                            }
                            else
                            {
                                pc++;
                            }
                        }
                        break;
                    case 6: //SUB
                        {
                            // Получение значений и типов операндов
                            byte op1Type = GetOperandType(commandFlags, 1);// куда записывать
                            int op1Value = GetValue(op1, op1Type);

                            byte op2Type = GetOperandType(commandFlags, 2);
                            int op2Value = GetValue(op2, op2Type); // откуда брать значения

                            // Установка в качестве значения первого операнда сумму  значений операндов
                            SetValue(op1, op1Type, op1Value - op2Value);
                            pc++;
                        }
                        break;
                    case 7: //AND
                        {
                            // Получение значений и типов операндов
                            byte op1Type = GetOperandType(commandFlags, 1);// куда записывать
                            int op1Value = GetValue(op1, op1Type);

                            byte op2Type = GetOperandType(commandFlags, 2);
                            int op2Value = GetValue(op2, op2Type); // откуда брать значения

                            // Установка в качестве значения первого операнда сумму  значений операндов
                            SetValue(op1, op1Type, op1Value & op2Value);
                            pc++;
                        }
                        break;
                    case 8: //OR
                        {
                            // Получение значений и типов операндов
                            byte op1Type = GetOperandType(commandFlags, 1);// куда записывать
                            int op1Value = GetValue(op1, op1Type);

                            byte op2Type = GetOperandType(commandFlags, 2);
                            int op2Value = GetValue(op2, op2Type); // откуда брать значения

                            // Установка в качестве значения первого операнда сумму  значений операндов
                            SetValue(op1, op1Type, op1Value | op2Value);
                            pc++;
                        }
                        break;
                    case 9: //XOR
                        {
                            // Получение значений и типов операндов
                            byte op1Type = GetOperandType(commandFlags, 1);// куда записывать
                            int op1Value = GetValue(op1, op1Type);

                            byte op2Type = GetOperandType(commandFlags, 2);
                            int op2Value = GetValue(op2, op2Type); // откуда брать значения

                            // Установка в качестве значения первого операнда сумму  значений операндов
                            SetValue(op1, op1Type, op1Value ^ op2Value);
                            pc++;
                        }
                        break;
                    case 10: //NOT
                        {
                            // Получение типов операндов
                            byte op1Type = GetOperandType(commandFlags, 1); // куда записывать
                            byte op2Type = GetOperandType(commandFlags, 2); // откуда брать значения
                            // Получение значений второго операнда
                            int value = GetValue(op2, op2Type);
                            // Установка значений первого операнда
                            SetValue(op1, op1Type, ~value);
                            // Переход к следующей команде
                            pc++;
                        }
                        break;
                    case 11: //INT
                        {
                            byte op1Type = GetOperandType(commandFlags, 1); // куда записывать
                            int op1Value = GetValue(op1, op1Type);
                            // Эмуляция системных прерываний
                            switch (op1Value)
                            {
                                case 0:
                                    Console.WriteLine("ВЫХОД");
                                    exitFlag = true;
                                    break;
                                case 1:
                                    Console.WriteLine($"Значения первого регистра: {registers[0]}");
                                    break;
                            }
                        }
                        pc++;
                        break;
                    default:
                        throw new Exception($"Неизвестная команда ({commandCode})");
                }
            }

            Console.WriteLine($"Значения всех регистров: {string.Join("; ", registers)}");
        }


        // Получение типа операнда
        private static byte GetOperandType(byte commandFlags, int operandNumber)
        {
            switch (operandNumber)
            {
                case 1:
                    return (byte)((commandFlags >> 6) & 0b11); //AND 11 
                case 2:
                    return (byte)((commandFlags >> 4) & 0b11);
                default:
                    throw new Exception($"Неверный номер ({operandNumber})");
            }
        }

        // Получение значения операнда
        private static int GetValue(byte source, byte sourceType)
        {
            switch (sourceType)
            {
                case 0b00: //R
                    return registers[source];
                case 0b01: //D
                    return memory[registers[source]]; // регистр хранит индек ячейки памяти
                case 0b10: //I
                    return source;
                case 0b11: //X
                    return memory[source];
                default:
                    throw new Exception($"Тип операнда - источника значения: {sourceType}");
            }
        }

        // Установка значения по адресу, задаваемому операндом
        private static void SetValue(byte destination, byte destinationType, int value)
        {
            switch (destinationType)
            {
                case 0b00: //R
                    registers[destination] = value;
                    break;
                case 0b01: //D
                    memory[registers[destination]] = value;
                    break;
                case 0b11: //X
                    memory[destination] = value;
                    break;
                default:
                    throw new Exception($"Неправильный тип операнда назначения: {destinationType}. Тип операнда назначения может быть только ссылочным.");
            }
        }

        public static void WriteMemory(int[] program, string[] command)
        {
            for (int index = 0; index < program.Length; index++)
            {
                memory[index] = program[index];
            }

            for (int index = 0; index < command.Length; index++)
            {
                cmemory[index] = MachineCode(command[index]);
            }

            pc = 0;
        }


        private static int MachineCode(String commandCode)
        {
            string[] command = commandCode.Split();

            String com = GetCommandBin(command[0]);
            String commandFlags = GetLiteralBin(command[1]);
            String op1 = Convert.ToString(int.Parse(command[2]), toBase: 2).PadLeft(8, '0');
            String op2 = Convert.ToString(int.Parse(command[3]), toBase: 2).PadLeft(8, '0');

            return Convert.ToInt32(com + commandFlags + op1 + op2, 2);

        }

        private static String GetCommandBin(String commandCode) => commandCode switch
        {
            "EMPTY" =>  "00000000",
            "MOV" =>    "00000001",
            "ADD" =>    "00000010",
            "CMP" =>    "00000011",
            "JNE" =>    "00000100",
            "JG" =>     "00000101",
            "SUB" =>    "00000110",
            "AND" =>    "00000111",
            "OR" =>     "00001000",
            "XOR" =>    "00001001",
            "NOT" =>    "00001010",
            "INT" =>    "00001011"
        };

        private static String GetLiteralBin(String commandCode) => commandCode switch
        {
            "RR" => "00000000",
            "RD" => "00010000",
            "RI" => "00100000",
            "RX" => "00110000",
            "DR" => "01000000",
            "DD" => "01010000",
            "DI" => "01100000",
            "DX" => "01110000",
            "IR" => "10000000",
            "ID" => "10010000",
            "II" => "10100000",
            "IX" => "10110000",
            "XR" => "11000000",
            "XD" => "11010000",
            "XI" => "11100000",
            "XX" => "11110000"
        };







        //------------------------------------Для вывода----------------------------------------------------
        // Типы операндов:
        // (0) - нулевой регистр (тип 0)
        // [(0)] - ячейка памяти с индексом, хранящимся в нулевом регистре (тип 1)
        // 0 - значение 0 (тип 2)
        // [0] - нулевая ячейка памяти (тип 3)
        // Вывод в консоль текущей исполняемой команды
        private static void PrintAsm(byte commandCode, byte commandFlags, byte op1, byte op2)
        {
            string commandName = GetCommandName(commandCode);
            byte op1Type = GetOperandType(commandFlags, 1);
            byte op2Type = GetOperandType(commandFlags, 2);
            int op1Value = GetValue(op1, op1Type);
            int op2Value = GetValue(op2, op2Type);
            Console.WriteLine($"{pc}: {commandName} {string.Format(GetTemplateString(op1Type), op1)} = {op1Value} {string.Format(GetTemplateString(op2Type), op2)} = {op2Value}");
        }

        // Метод для получения имени команды по её коду
        private static string GetCommandName(byte commandCode) => commandCode switch
        {
            0 => "EMPTY",
            1 => "MOV",
            2 => "ADD",
            3 => "CMP",
            4 => "JNE",
            5 => "JG",
            6 => "SUB",
            7 => "AND",
            8 => "OR",
            9 => "XOR",
            10 => "NOT",
            11 => "INT",
            _ => "UNKNOWN"
        };

        // Получение шаблонной строки для типа операнда
        private static string GetTemplateString(byte operandType) =>
            operandType switch
            {
                0b00 => "({0})",
                0b01 => "[({0})]",
                0b10 => "{0}",
                0b11 => "[{0}]"
            };
    }

    //
    //

    //
    //

    //
    //

    //
    //

    //
    //

    //
    //
}
