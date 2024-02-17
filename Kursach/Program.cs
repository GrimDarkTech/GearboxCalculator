using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursach
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            bool smartCalculate = false;
            double outPower = 1200;
            double rpm = 90;
            

            double efficiencyFactor = CalculateEfficiency(outPower, rpm);
            double motorPower = CalculeteMotorPower(outPower, efficiencyFactor);
            double[] motorDeterminationValues = MotorDetermination(rpm, motorPower);

            double gearRatioReductorSelected = motorDeterminationValues[0];
            double gearRatioChainSelected = motorDeterminationValues[1];
            double gearRatioReductor = motorDeterminationValues[2];
            double gearRatioChain = motorDeterminationValues[3];
            double motorRPM = motorDeterminationValues[4];
            double motorShaftDiameter = motorDeterminationValues[5];
            double[] kinematicsValues;
            if (smartCalculate)
            {
                Log("\nДля: uред = " + gearRatioReductorSelected + "; uвн = " + gearRatioChainSelected + ";\n");
                kinematicsValues = CalculateShaftKinematics(rpm, outPower, gearRatioChainSelected, motorRPM, gearRatioReductorSelected);
            }
            else
            {
                Log("\nДля: uред = " + gearRatioReductor + "; uвн = " + gearRatioChain + ";\n");
                kinematicsValues = CalculateShaftKinematics(rpm, outPower, gearRatioChain, motorRPM, gearRatioReductor);
            }

            double rpm1 = kinematicsValues[0];
            double angularVelocity1 = kinematicsValues[1];
            double torque1 = kinematicsValues[2];
            double rpm2 = kinematicsValues[3];
            double angularVelocity2 = kinematicsValues[4];
            double torque2 = kinematicsValues[5]; 
            double rpm3 = kinematicsValues[6];
            double angularVelocity3 = kinematicsValues[7];
            double torque3 = kinematicsValues[8];

            double[] gearValues;
            if (smartCalculate)
            {
                gearValues = CalculateGear(gearRatioReductorSelected, torque2, torque1, angularVelocity1, angularVelocity2);
            }
            else
            {
                gearValues = CalculateGear(gearRatioReductor, torque2, torque1, angularVelocity1, angularVelocity2);
            }
            double gearDiameter = gearValues[0];
            double gearWidth = gearValues[1];
            double gearTeethTopDiameter = gearValues[2];
            double gearTeethBottomDiameter = gearValues[3];
            double wheelDiameter = gearValues[4];
            double wheelWidth = gearValues[5];
            double wheelTeethTopDiameter = gearValues[6];
            double wheelTeethBottomDiameter = gearValues[7];
            double module = gearValues[8];
            double centerToCenterDistance = gearValues[9];


            PreCalculateShaft(motorShaftDiameter, torque2, wheelWidth, torque1, gearWidth, module, centerToCenterDistance);

            CalculateChainDrive(gearRatioChain, rpm2, outPower, angularVelocity2, torque2);

            Console.ReadLine();
        }

        public static double CalculateEfficiency(double outPower, double rpm)
        {
            //КПД привода
            Log("1. Определение КПД привода: \n");
            Log("Угловая скорость на выходном валу");
            Log("ωвых = π*n / 30;");
            double outAngularVelocity = Math.PI * rpm / 30;
            Log("ωвых = " + outAngularVelocity + " рад/с. \n");
            Log("Вращающий момент на выходном валу");
            Log("Tвых = Pвых / ωвых;");
            double outTorque = outPower / outAngularVelocity;
            Log("Tвых = " + outTorque + " н*м; \n");
            Log("Общий КПД привода");
            Log("ηΣ = ηзк * ηп^k * ηцп * ηм;");
            Log("ηΣ = 0,98 * 0,995^3 * 0,93 * 0,98;");
            double efficiencyFactor = 0.98 * Math.Pow(0.995, 3) * 0.93 * 0.98;
            Log("ηΣ = " + efficiencyFactor + "; \n");
            return efficiencyFactor;
        }
        public static double CalculeteMotorPower(double outPower, double efficiencyFactor)
        {
            //Требуемая мощность электродвигателя
            Log("2. Определние требуемой мощности двигателя \n");
            Log("Pтр = Pд = Pвых / ηΣ;");
            double motorPower = outPower / efficiencyFactor;
            Log("Pтр = " + motorPower + " Вт; \n");
            return motorPower;
        }
        public static double[] MotorDetermination(double rpm, double motorPower)
        {
            double[] values = new double[6];
            //Определение электродвигателя
            Log("3. Определние электродвигателя \n");
            Log("nдв = nвых * uΣ;");
            Log("uΣ = uред * uвн; \n");
            Log("Таблица 3.2 \n u   рек.знач наибольш.знач \n uзк   3 - 6    12.5\n uцп   2 - 6    8\n");
            Log("nдв = nвых * (uредmin .. uредmax)(uвнmin .. uвнmax)");
            Log("nдвmax = nвых * (uредmax * uвнmax)");
            double motorRPMmax = 36 * rpm;
            Log("nдвmax = " + motorRPMmax + "об/мин;");
            Log("nдвmin = nвых * (uредmin * uвнmin)");
            double motorRPMmin = 6 * rpm;
            Log("nдвmin = " + motorRPMmin + "об/мин;");
            Log("nдвср = (nдвmax + nдвmin) / 2");
            double motorRPMav = (motorRPMmax + motorRPMmin) / 2;
            Log("nдвср = " + motorRPMav + " об/мин.");
            Motor selectedMotor = GetMotor(motorPower, motorRPMav);
            Log("По табл. в приложении 3 \nТип электродвигателя: " + selectedMotor.type + " / " + selectedMotor.rpm + " об/мин;\n");
            Log("Определение фактического передаточного числа привода");
            Log("uΣф = nдвф / nвых;");
            double gearRatioReal = selectedMotor.rpm / rpm;
            Log("uΣф = " + gearRatioReal + ";");
            double gearRatioReductor = 3;
            double gearRatioChain = 1;
            Log("uвн = uΣф / uред; \n");
            double ratioDeltaMin = 24;
            double gearRatioReductorSelected = gearRatioReductor;
            double gearRatioChainSelected = gearRatioReal / gearRatioReductor;
            for (int i = 6; i > 2; i--)
            {
                gearRatioReductor = i;
                gearRatioChain = gearRatioReal / gearRatioReductor;
                if (gearRatioChain >= 2 && gearRatioChain < 6)
                {
                    Log("uред = " + gearRatioReductor + "; uвн = " + gearRatioChain + ";");
                    if(Math.Abs(gearRatioReductor - gearRatioChain) <= ratioDeltaMin)
                    {
                        ratioDeltaMin = Math.Abs(gearRatioReductor - gearRatioChain);
                        gearRatioReductorSelected = gearRatioReductor;
                        gearRatioChainSelected = gearRatioChain;
                    }
                }
            }
            values[0] = gearRatioReductorSelected;
            values[1] = gearRatioChainSelected;
            values[2] = gearRatioReductor;
            values[3] = gearRatioChain;
            values[4] = selectedMotor.rpm;
            values[5] = selectedMotor.diameterShaft;
            return values;
        }
        public static double[] CalculateShaftKinematics(double rpm, double outPower, double gearRatioChain, double motorRPM, double gearRatioReductor)
        {
            double[] values = new double[9];
            Log("3.1. Расчет угловых скоростей, чисел оборотов и вращающих моментов \n");
            double rpm3 = rpm;
            double angularVelocity3 = Math.PI * rpm3 / 30;
            double torque3 = outPower / angularVelocity3;
            Log("C(3) n3 = nзад = " + rpm3 + "об/мин      ω3 = π * n3 / 30 = " + angularVelocity3 + "рад/c   T3 = P3 / ω3 = " + torque3 + " н*м;");
            double rpm2 = rpm3 * gearRatioChain;
            double angularVelocity2 = angularVelocity3 * gearRatioChain;
            double torque2 = outPower / (angularVelocity2 * 0.93 * 0.995);
            Log("B(2) n2 = n3 * uвн = " + rpm2 + "об/мин   ω2 = ω3 * uвн = " + angularVelocity2 + "рад/c   T2 = P3 / (ω2 * η3 * η2) = " + torque2 + " н*м;");
            double rpm1 = motorRPM;
            double angularVelocity1 = Math.PI * motorRPM / 30;
            double torque1 = torque2 / (gearRatioReductor * 0.98 * Math.Pow(0.995, 3));
            Log("A(1) n1 = nдв = " + rpm1 + "об/мин      ω1 = π * nдв / 30 = " + angularVelocity1 + "рад/c   T1 = T2 / (uред * η1 * η2^3) = " + torque1 + " н*м;");
            values[0] = rpm1;
            values[1] = angularVelocity1;
            values[2] = torque1;
            values[3] = rpm2;
            values[4] = angularVelocity2;
            values[5] = torque2;
            values[6] = rpm3;
            values[7] = angularVelocity3;
            values[8] = torque3;
            return values;
        }
        public static double[] CalculateGear(double gearRatioReductor, double torque2, double torque1, double angularVelocity1, double angularVelocity2)
        {
            double[] gearValues = new double[10];
            Log("\n4. Расчет зубчатых колес редуктора \n");
            Log("Материал - Сталь 45");
            Log("Твердость шестерни HB1 = 260 .. 320, твердость колеса HB2 = HB1 - 30");
            Log("HB1 = 300; HB2 = 270");
            double hardnessGear = 300;
            double hardnessWheel = hardnessGear - 30;
            Log("\n1)Допускаемое контаткое напряжение \nσн = σHlimb * Khl / [SH]");
            Log("Термическая обработка: Отжиг, нормализация,улучшение \nСталь: Углеродистая сталь\nТвердость < HB350\nσHlimb = 2HB + 70\nσFlimb = HB + 260");
            Log("Khl = 1 - коэффицент долговечности");
            Log("[SH] = 1.1 - коэффицент безопасности. 1.1 - для экономии металла!");
            double contactStressGear = (2 * hardnessGear + 70) / 1.1;
            Log("σн1 = " + contactStressGear);
            double contactStressWheel = (2 * hardnessWheel + 70) / 1.1;
            Log("σн2 = " + contactStressWheel);
            Log("Расчет пределов контактной выносливости:");
            Log("[σн] = 0.9 * σHlimb / [SH]");
            double enduranceLimitGear = 0.9 * (2* hardnessGear + 70) / 1.1;
            Log("[σн1] = " + enduranceLimitGear);
            double enduranceLimitWheel = 0.9 * (2 * hardnessWheel + 70) / 1.1;
            Log("[σн2] = " + enduranceLimitWheel);
            Log("Меньшее - [σн2]; [σн] = [σн2] = " + enduranceLimitWheel);
            Log("\n2)Выбор коэффециента ширины венца \nΨba = b / aw; Ψba = 0.4 - редуктор общнего назначения");
            Log("\n3)Межосевое растояние из условия контактной выносливости активных поверхностей зубъев по формула");
            Log("aw = Ka * (uред + 1) * √(3)(T2 * Khβ * 1000) / ([σн]^2*uред^2*Ψba); \nKa = 49.5 - прямозубая передача");
            Log("Ψbd = 0.5 * Ψba * (uред + 1) = " + 0.5 * 0.4 * (gearRatioReductor + 1));
            double Fbd = 0.5 * 0.4 * (gearRatioReductor + 1);
            Log("Прямозубая одноступенчатая передача, колесо расположено в средней части вала, твердость HB < 350 и Ψbd = " + Fbd);
            double Khb = -69420;
            switch (Fbd)
            {
                case 0.2:
                    Khb = 1.01;
                    break;
                case 0.4:
                    Khb = 1.02;
                    break;
                case 0.6:
                    Khb = 1.025;
                    break;
                case 0.8:
                    Khb = 1.025;
                    break;
                case 1:
                    Khb = 1.03;
                    break;
                case 1.2:
                    Khb = 1.04;
                    break;
            }
            Log("Khβ = " + Khb);
            double centerToCenterDistance = 49.5 * (gearRatioReductor + 1) * Math.Pow(((torque2 * Khb * 1000) / (Math.Pow(enduranceLimitWheel, 2) * Math.Pow(gearRatioReductor, 2) * 0.4)), 0.333333);
            Log("aw = " + centerToCenterDistance);
            if (centerToCenterDistance <= 40)
                centerToCenterDistance = 40;
            else if (centerToCenterDistance <= 50)
                centerToCenterDistance = 50;
            else if (centerToCenterDistance <= 63)
                centerToCenterDistance = 63;
            else if (centerToCenterDistance <= 80)
                centerToCenterDistance = 80;
            else if (centerToCenterDistance <= 100)
                centerToCenterDistance = 100;
            else if (centerToCenterDistance <= 125)
                centerToCenterDistance = 125;
            else if (centerToCenterDistance <= 160)
                centerToCenterDistance = 160;
            else if (centerToCenterDistance <= 200)
                centerToCenterDistance = 200;
            else if (centerToCenterDistance <= 315)
                centerToCenterDistance = 315;
            else if (centerToCenterDistance <= 400)
                centerToCenterDistance = 400;
            else if (centerToCenterDistance <= 500)
                centerToCenterDistance = 500;
            else if (centerToCenterDistance <= 630)
                centerToCenterDistance = 630;
            else if (centerToCenterDistance <= 800)
                centerToCenterDistance = 800;
            else if (centerToCenterDistance <= 1000)
                centerToCenterDistance = 1000;
            else if (centerToCenterDistance <= 1250)
                centerToCenterDistance = 1250;
            Log("Межосевое расстояние округляем до aw = " + centerToCenterDistance + " по ГОСТ 2185-81");
            Log("\n4)Модуль зацепления принимается по рекомендации: m = (0.01 .. 0.02) * aw");
            double module = 0.015 * centerToCenterDistance;
            Log("m = " + module);
            if (module <= 0.25)
                module = 0.25;
            else if (module <= 0.3)
                module = 0.3;
            else if (module <= 0.4)
                module = 0.4;
            else if (module <= 0.5)
                module = 0.5;
            else if (module <= 0.6)
                module = 0.6;
            else if (module <= 0.8)
                module = 0.8;
            else if (module <= 1)
                module = 1;
            else if (module <= 1.25)
                module = 1.25;
            else if (module <= 1.5)
                module = 1.5;
            else if (module <= 2)
                module = 2;
            else if (module <= 2.5)
                module = 2.5;
            else if (module <= 3)
                module = 3;
            else if (module <= 4)
                module = 4;
            else if (module <= 5)
                module = 5;
            else if (module <= 6)
                module = 6;
            else if (module <= 8)
                module = 8;
            else if (module <= 10)
                module = 10;
            else if (module <= 12)
                module = 12;
            else if (module <= 12)
                module = 12;
            else if (module <= 16)
                module = 16;
            else if (module <= 20)
                module = 20;
            Log("Модуль зацепления округляем до m = " + module + " по ГОСТ 9563-80");
            Log("\n5)Число зубъев шестерни для прямозубых колес определяется из соотношения: z1 = 2 * aw / (uред + 1) * m");
            double numberOfGearTeeth = (2 * centerToCenterDistance) / ((gearRatioReductor + 1) * module);
            Log("z1 = " + numberOfGearTeeth + "; округляем до целого z1 = " + Math.Round(numberOfGearTeeth));
            numberOfGearTeeth = Math.Round(numberOfGearTeeth);
            if (numberOfGearTeeth > 17)
                Log("Число зубъев больше 17 - подрезание ножки и коррегирование не требуется");
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Log("Число зубъев меньше 17 - требуется подрезание ножки и коррегирование!!!");
                Console.ForegroundColor = ConsoleColor.White;
            }
            if (numberOfGearTeeth >= 22 && numberOfGearTeeth <= 36)
                Log("Число зубъев больше попадает в диапазон [22 .. 36] рекомендованный для первой ступени");
            else 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Log("Число зубъев больше НЕ попадает в диапазон [22 .. 36] рекомендованный для первой ступени!!!");
                Console.ForegroundColor = ConsoleColor.White;
            } 
            Log("\n6)Число зубъев колеса: z2 = z1 * uред;");
            double numberOfWheelTeeth = numberOfGearTeeth * gearRatioReductor;
            Log("z2 = " + numberOfWheelTeeth);
            Log("\n7)Проверка фактического передаточного числа: uф = z2 / z1;");
            Log("uф = " + gearRatioReductor);
            Log("\n8)Определение основных размеров шестерни и колеса:");
            Log("-диаметры делительные");
            double gearDiameter = numberOfGearTeeth * module;
            double wheelDiameter = numberOfWheelTeeth * module;
            Log(" d1 = m * z1 = " + gearDiameter + "\n d2 = m * z2 = " + wheelDiameter);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Log("Проверка по определенному ранее межосевому расстоянию:");
            Log(" aw = 0.5 * (d1 + d2); aw = " + 0.5 * (gearDiameter + wheelDiameter) + " ≈ " + centerToCenterDistance);
            centerToCenterDistance = 0.5 * (gearDiameter + wheelDiameter);
            Log("Принимаем значение межосевого расстояния aw = " + centerToCenterDistance + " мм;");
            Console.ForegroundColor = ConsoleColor.White;
            Log("-диаметры вершин зубъев");
            double gearTeethTopDiameter = gearDiameter + 2 * module;
            double wheelTeethTopDiameter = wheelDiameter + 2 * module;
            Log(" da1 = d1 + 2 * m = " + gearTeethTopDiameter + "\n da2 = d2 + 2 * m = " + wheelTeethTopDiameter);
            Log("-диаметры впадин зубъев");
            double gearTeethBottomDiameter = gearDiameter - 2.5 * module;
            double wheelTeethBottomDiameter = wheelDiameter - 2.5 * module;
            Log(" df1 = d1 - 2.5 * m = " + gearTeethBottomDiameter + "\n df2 = d2 - 2.5 * m = " + wheelTeethBottomDiameter);
            Log("-ширина колеса");
            double wheelWidth = 0.4 * centerToCenterDistance;
            Log(" b2 = Ψba * aw = " + wheelWidth);
            Log("-ширина шестерни");
            double gearWidth = wheelWidth + 5;
            Log(" b1 = b2 + 5 = " + gearWidth);
            if (wheelWidth < gearDiameter)
                Log("Условие b2 < d1 выполняется; " + wheelWidth + " < " + gearDiameter);
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Log("Условие b2 < d1 НЕ выполняется!!!; " + wheelWidth + " >= " + gearDiameter + "!!!");
                Console.ForegroundColor = ConsoleColor.White;
            }
            wheelWidth = roundWidth(wheelWidth);
            gearWidth = roundWidth(gearWidth);
            Log("Округление b1 и b2 по ряду чисел [16]: b1 = " + gearWidth + "; b2 = " + wheelWidth);
            double linearVelocityGear = 0.5 * angularVelocity1 * gearDiameter * 0.001;
            double linearVelocityWheel = 0.5 * angularVelocity2 * wheelDiameter * 0.001;
            Log("Расчет окружной скорости колес: \nv1 = 0.5 * ω1 * d1 = " + linearVelocityGear + " м/c\nv2 = 0.5 * ω2 * d2 = " + linearVelocityWheel +" м/c");
            Log(" v1 = v2;");
            Log("\n9)Коэфф. нагрузки");
            Log("Kн = Khβ * Kha * Khv");
            Log("По таблице 4.3 определяем степень точности передачи:");
            if (linearVelocityWheel <= 4)
                Log(" степень точности - 9 (пониженная точность), по рекомендации берем - 8 (средней точности)");
            else if (linearVelocityWheel <= 6)
                Log(" степень точности - 8 (средней точности), по рекомендации берем - 7 (точная)");
            else if (linearVelocityWheel <= 12)
                Log(" степень точности - 7 (точная), по рекомендации берем - 6 (высокоточная)");
            else if (linearVelocityWheel <= 18)
                Log(" степень точности - 6 (высокоточная)");
            Log("Kha = 1 - прямозубые колеса;");
            Log("Khv = 1 - передача общего назначения;");
            double Kn = Khb * 1 * 1;
            Log("Kн = " + Kn);
            Log("\n10)Проверка контактных напряжений");
            Log("σн = 270 / aw * √(T2 * 1000 * Kн * (uред + 1)^3 / b2 * uред^2) < [σн]");
            double qn = (270 / centerToCenterDistance) * Math.Sqrt(torque2 * 1000 * Kn * Math.Pow((gearRatioReductor + 1) , 3) / (wheelWidth * Math.Pow(gearRatioReductor, 2)));
            if (qn < enduranceLimitWheel)
            {
                Log("σн = " + qn + " < " + enduranceLimitWheel + " проверка пройдена");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Log("σн = " + qn + " >= " + enduranceLimitWheel + " проверка НЕ пройдена!!!");
                Console.ForegroundColor = ConsoleColor.White;
            }
            Log("\n11)Силы, действующие в зацеплении:");
            double forceTan = 2 * torque1 / (gearDiameter * 0.001);
            Log("окружная Ft = 2T1 / d1 = " + forceTan);
            double forceRad = forceTan * Math.Tan(20 * Math.PI / 180);
            Log("окружная Fr = Ft * tgα = " + forceRad + "; α = 20 - стандартный угол зацепления");
            Log("\n12)Проверка зубъев на выносливость по напряжениям изгиба");
            Log("σf = (Ft * Kf * Yf * Yβ * Kfa) / (b1 * m) < [σf]");
            double Kf = 1.3;
            Log("Kf = 1.3 - коэффициент нагрузки для симмертричного колеса отностилено опор;");
            double Yf = SelectYf(numberOfGearTeeth);
            Log("Yf = "+ Yf + " - коэффициент, учитывающий форму зуба, выбирается по табл 4.7;");
            double Yb = 1;
            Log("Yb = " + Yb + " - коэффициент, учитывающий наклон зуба. Для прямозубых = 1;");
            double Kfa = 1;
            Log("Kfa = " + Kfa + " - коэффициент распределения нагрузки,принимается равным для прямозубых KFa = 1; ");
            double qf = (forceTan * Kf * Yf * Yb * Kfa) / (gearWidth * module);
            Log("σf = " + qf);
            Log("[σf]  = σ_1 / [Sf] * Kσ - допускаемое напряжение изгиба");
            Log("σ_1 = 0.43 * σв - для углеродистых сталей");
            Log("σв = 570 МПа - для стали 45 при нормализации или улучшении");
            Log("[Sf] = 1.5 - по табл. 4.8, по рекомендации 3 строка (Поковки стальные нормализованные или улучшенные);");
            Log("Kσ = 1.5 - по табл. 4.9, по рекомендации 1 строка (Стальные нормализованные или улучшенные, а также с поверхностной закалкой); ");
            double acceptable_qf = 0.43 * 570 / (1.5 * 1.5);
            Log("[σf] = " + acceptable_qf);
            if (qf < acceptable_qf)
                Log("Проверка пройдена σf = " + qf + " < [σf] = " + acceptable_qf);
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Log("Проверка НЕ пройдена! σf = " + qf + " > = [σf] = " + acceptable_qf + "!!!");
                Console.ForegroundColor = ConsoleColor.White;
            }
            gearValues[0] = gearDiameter;
            gearValues[1] = gearWidth;
            gearValues[2] = gearTeethTopDiameter;
            gearValues[3] = gearTeethBottomDiameter;
            gearValues[4] = wheelDiameter;
            gearValues[5] = wheelWidth;
            gearValues[6] = wheelTeethTopDiameter;
            gearValues[7] = wheelTeethBottomDiameter;
            gearValues[8] = module;
            gearValues[9] = centerToCenterDistance;
            return gearValues;
        }
        public static void PreCalculateShaft(double diameterMotor, double torque2, double wheelWidth, double torque1, double gearWidth, double module, double centerToCenterDistance)
        {
            Log("\n5 Предварительный расчет валов. Конструирование валов \n");
            Log("\n5.1 Выбора материала вала");
            Log("Выбран материал: Сталь 45");
            Log("\n5.2 Предварительное определение диаметров шеек вала");
            Log("dв =  √(3)((T * 1000) / (0.2 * [τ])) - диаметр выходного конца вала \n где T - вращающий момент на валу, [τ] = 32.5 Н/мм^2 - допускаемые напряжения кручения");
            Log("\nВедущий вал:");
            Log("Вал редуктора соединен муфтой с валом электродвигателя. dв1 = (0,7-1)dдв");
            double diameterShaft1 = 0.78 * diameterMotor;
            Log("dв1 = 0.78 * " + diameterMotor + " = " + diameterShaft1 + " мм;");
            Log("\nВедомый вал:");
            Log("dв2 =  √(3)((T2 * 1000) / (0.2 * [τ]))");
            double diameterShaft2 = Math.Pow((torque2 * 1000) / (0.2 * 32.5), 0.333333);
            Log("dв2 = " + diameterShaft2 + " мм;");
            Log("\n1)Диаметр шеек под подшипники (dпш): \n dпш = dвнутркольца > dв");
            Log("Шариковые радиальные подшипники. Легкая серия");
            Bearing bearingShaft1 = GetBearing(diameterShaft1);
            Log("Ведущий вал: \n dв1 = " + diameterShaft1 + "; Выбран подшипник: " + bearingShaft1.name + " dшп1 = " + bearingShaft1.diameterInner + " мм; dшпв1 = " + bearingShaft1.diameterOuter + " мм; B1 = " + bearingShaft1.width);
            Bearing bearingShaft2 = GetBearing(diameterShaft2);
            Log("Ведомый вал: \n dв2 = " + diameterShaft2 + "; Выбран подшипник: " + bearingShaft2.name + " dшп2 = " + bearingShaft2.diameterInner + " мм; dшпв2 = " + bearingShaft2.diameterOuter + " мм; B2 = " + bearingShaft2.width);
            Log("\n2)Диаметр шейки под зубчатым колесом (dк):");
            Log("dк = √(3)((T2 * 1000) / (0.2 * [τ])) , [τ] = 15 Н/мм^2");
            double diameterShaftWheel = Math.Pow((torque2 * 1000) / (0.2 * 15), 0.333333);
            Log("dк = " + diameterShaftWheel + " ≈ " + Math.Round(diameterShaftWheel) + ";");
            diameterShaftWheel = Math.Round(diameterShaftWheel);
            Log("\n3)Расчет буртиков:");
            Log("Вал - ступенчатый. Высота буртиков определяет диаметр большей шейки вал dбол = dмен + 2h");
            double collarHeight = (diameterShaftWheel - bearingShaft2.diameterInner) / 2;
            Log("h = " + collarHeight);
            Log("\n4-5)Радиусы галтелей и катеты фасок:");
            Log("Подшипники ведущего вала - " + bearingShaft1.name);
            Log("d = " + bearingShaft1.diameterInner + " мм;" + " С = " + bearingShaft1.chamfer + " мм;" + " R = " + (bearingShaft1.chamfer - 0.5) + " мм;");
            Log("Подшипники ведомого вала - " + bearingShaft2.name);
            Log("d = " + bearingShaft2.diameterInner + " мм;" + " С = " + bearingShaft2.chamfer + " мм;" + " R = " + (bearingShaft2.chamfer - 0.5) + " мм;");
            Log("Колесо");
            if(diameterShaftWheel <= 15)
            Log("d = " + diameterShaftWheel + " мм;" + " С = " + 1.5 + " мм;" + " R = " + 1 + " мм;");
            else if (diameterShaftWheel <= 40)
                Log("d = " + diameterShaftWheel + " мм;" + " С = " + 2 + " мм;" + " R = " + 1.5 + " мм;");
            else if (diameterShaftWheel <= 80)
                Log("d = " + diameterShaftWheel + " мм;" + " С = " + 3 + " мм;" + " R = " + 2 + " мм;");
            else Log("d = " + diameterShaftWheel + " мм;" + " С = " + 4 + " мм;" + " R = " + 2.5 + " мм;");
            Log("\n5.4 Выбор посадок деталей передач на шейки вала");
            Log("-зубчатое колесе (H7/r6)");
            Log("-звездочка (H7/h6)");
            Log("-муфта (H8/k6)");
            Log("-внутренние кольца подшипников качения (H7/k6)");
            Log("-наружные кольца подшипников качения (H7/h6)");
            Log("\n5.5 Выбор расстояния между опорами валов");
            Log("Цилиндрический одноступенчатый редуктор: \n l = Lст +2x +W где Lст – длина ступицы колеса, равная b2 +(5-10) мм, x - = 8 - 15 мм – зазор между торцом ступицы зубчатого колеса и внутренними стенками корпуса редуктора, W – ширина стенки корпуса редуктора в месте установки подшипников");
            double hubLenght = wheelWidth + 5;
            Log("Lст = b2 + 5 = " + hubLenght);
            Log("x = 10 мм;");
            double[] lineralSizes = SelectSize(torque2, bearingShaft2.width);
            double wallWidth2 = lineralSizes[0];
            double tableFValue2 = lineralSizes[1];
            lineralSizes = SelectSize(torque1, bearingShaft1.width);
            double wallWidth1 = lineralSizes[0];
            double tableFValue1 = lineralSizes[1];
            Log("По таблице 5.1 - линейные размеры для расчета валов");
            Log("T1 = " + torque1 + " f1 = " + tableFValue1 + " W = " + wallWidth1);
            Log("T2 = " + torque2 + " f2 = " + tableFValue2 + " W = " + wallWidth2);
            Log("\n6 Определение конструктивных размеров зубчатых колес \n");
            double hubDiameter = diameterShaftWheel * 1.6;
            Log("Диаметр ступицы: dст = 1.6 * dк = " + hubDiameter + " мм");
            Log("Длина ступицы: Lст = b2 + 5 = " + hubLenght + " мм");
            double rimWidth = 3 * module;
            Log("Толщина обода: b0 = 3 * m = " + rimWidth + " мм");
            double diskWidth = 0.3 * wheelWidth;
            Log("Толщина обода: C = 0.3 * b2 = " + diskWidth + " мм");
            Log("\n7 Определение размеров конструктивных элементов корпуса редуктора\n");
            double wallWidth = 0.025 * centerToCenterDistance + 1;
            double coverWidth = 0.02 * centerToCenterDistance + 1;
            Log("Толщина стенок корпуса b = 0.025 * аW + 1 = " + wallWidth + " мм;");
            Log("Толщина крышки bк = 0.02 * аW + 1 = " + coverWidth + " мм;");
            if (wallWidth < 8)
            {
                wallWidth = 8;
                coverWidth = 6;
            }
            Log("По рекомендации для отливки b = 8 мм, bк = 6 мм.");
            Log("Толщина фланцев поясов корпуса и крышки:");
            Log("-верхнего пояса корпуса и пояса крышки bкф = 1.5 * b = " + 1.5 * wallWidth + " мм; bкрф = 1.5 * bк = " + 1.5 * coverWidth + " мм;");
            Log("- нижнего пояса (опорной поверхности) корпуса р = 2.35 * b = " + 2.35 * wallWidth + " мм;");
            Log("Диаметр болтов:");
            double boltFundament = (0.033 * centerToCenterDistance) + 12;
            Log("- фундаментных dф = 0.33 * aw + 12 = " + boltFundament + " мм;");
            double boltBearing = 0.75 * boltFundament;
            Log("- соединяющих крышку с корпусом у гнезд подшипников dк = 0.75 * dф = " + boltBearing + " мм;");
            double boltCover = 0.5 * boltFundament;
            Log("- соединяющих крышку с корпусом dкк = 0.5 * dф = " + boltCover + " мм;");
        }

        public static void CalculateChainDrive(double chainGearRatio, double rpm2, double outPower, double angularVelocity2, double torque2)
        {
            Log("\n9.1 Расчет цепной передачи\n");
            Log("Выбор числа зубьев меньшей звездочки:");
            Log("Втулочная и роликовая цепь; uвн = " + chainGearRatio);
            Log("По таблице 9.1 выбирается число зубъев меньшей звездочки по передаточному числу uвн");
            double numberOfteethSprocket1 = 13;
            if (chainGearRatio > 1 && chainGearRatio < 2)
            {
                numberOfteethSprocket1 = 27;
            } 
            else if (chainGearRatio >= 2 && chainGearRatio < 3)
            {
                numberOfteethSprocket1 = 25;
            }
            else if (chainGearRatio >= 3 && chainGearRatio < 4)
            {
                numberOfteethSprocket1 = 23;
            }
            else if (chainGearRatio >= 4 && chainGearRatio < 5)
            {
                numberOfteethSprocket1 = 21;
            }
            else if (chainGearRatio >= 5 && chainGearRatio < 6)
            {
                numberOfteethSprocket1 = 17;
            }
            else if (chainGearRatio == 6)
            {
                numberOfteethSprocket1 = 15;
            }
            Log("z1 = " + numberOfteethSprocket1 + ";");
            Log("По таблице 9.2 выбирается шаг цепи t, мм в зависимости от выбранного числа зубьев");
            double chainStep = 15.875;
            if(numberOfteethSprocket1 < 9)
            {
                chainStep = 8;
            }
            else if(numberOfteethSprocket1 <= 10)
            {
                chainStep = 9.525;
            }
            else if (numberOfteethSprocket1 <= 11)
            {
                chainStep = 12.7;
            }
            else if (numberOfteethSprocket1 <= 12)
            {
                chainStep = 15.875;
            }
            else if (numberOfteethSprocket1 <= 13)
            {
                chainStep = 19.05;
            }
            else if (numberOfteethSprocket1 <= 14)
            {
                chainStep = 25.4;
            }
            else if (numberOfteethSprocket1 <= 15)
            {
                chainStep = 31.75;
            }
            else if (numberOfteethSprocket1 <= 17)
            {
                chainStep = 38.1;
            }
            else if (numberOfteethSprocket1 <= 19)
            {
                chainStep = 44.45;
            }
            else if (numberOfteethSprocket1 <= 20)
            {
                chainStep = 50.08;
            }
            Log("Выбран шаг цепи t = " + chainStep);
            Log("Число зубьев ведомой (большей) звездочки: \nz2 = z1 * uвн;");
            double numberOfteethSprocket2 = numberOfteethSprocket1 * chainGearRatio;
            numberOfteethSprocket2 = Math.Round(numberOfteethSprocket2 + 0.4);
            Log("z2 = " + (numberOfteethSprocket1 * chainGearRatio) + " ≈ " + numberOfteethSprocket2 + ";");
            Log("По таблице 9.3 определяется допускаемое среднее давление в шарнире цепи [p] в зависимости от шага цепи:");
            double allowablePressure = 13.4;
            if(rpm2 <= 50)
            {
                if(chainStep >= 12.7 && chainStep <= 15.87)
                {
                    allowablePressure = 34.3;
                }
                if (chainStep >= 19.05  && chainStep <= 25.04)
                {
                    allowablePressure = 34.3;
                }
                if (chainStep >= 31.75 && chainStep <= 38.1)
                {
                    allowablePressure = 34.3;
                }
                if (chainStep >= 44.45 && chainStep <= 50.8)
                {
                    allowablePressure = 34.3;
                }
            }
            else if (rpm2 <= 200)
            {
                if (chainStep >= 12.7 && chainStep <= 15.87)
                {
                    allowablePressure = 30.9;
                }
                if (chainStep >= 19.05 && chainStep <= 25.04)
                {
                    allowablePressure = 29.4;
                }
                if (chainStep >= 31.75 && chainStep <= 38.1)
                {
                    allowablePressure = 28.1;
                }
                if (chainStep >= 44.45 && chainStep <= 50.8)
                {
                    allowablePressure = 25.7;
                }
            }
            else if (rpm2 <= 400)
            {
                if (chainStep >= 12.7 && chainStep <= 15.87)
                {
                    allowablePressure = 28.1;
                }
                if (chainStep >= 19.05 && chainStep <= 25.04)
                {
                    allowablePressure = 25.7;
                }
                if (chainStep >= 31.75 && chainStep <= 38.1)
                {
                    allowablePressure = 23.7;
                }
                if (chainStep >= 44.45 && chainStep <= 50.8)
                {
                    allowablePressure = 20.6;
                }
            }
            else if (rpm2 <= 600)
            {
                if (chainStep >= 12.7 && chainStep <= 15.87)
                {
                    allowablePressure = 25.7;
                }
                if (chainStep >= 19.05 && chainStep <= 25.04)
                {
                    allowablePressure = 22.9;
                }
                if (chainStep >= 31.75 && chainStep <= 38.1)
                {
                    allowablePressure = 20.6;
                }
                if (chainStep >= 44.45 && chainStep <= 50.8)
                {
                    allowablePressure = 17.2;
                }
            }
            else if (rpm2 <= 800)
            {
                if (chainStep >= 12.7 && chainStep <= 15.87)
                {
                    allowablePressure = 23.7;
                }
                if (chainStep >= 19.05 && chainStep <= 25.04)
                {
                    allowablePressure = 20.6;
                }
                if (chainStep >= 31.75 && chainStep <= 38.1)
                {
                    allowablePressure = 18.1;
                }
                if (chainStep >= 44.45 && chainStep <= 50.8)
                {
                    allowablePressure = 14.7;
                }
            }
            else if (rpm2 <= 1000)
            {
                if (chainStep >= 12.7 && chainStep <= 15.87)
                {
                    allowablePressure = 22.0;
                }
                if (chainStep >= 19.05 && chainStep <= 25.04)
                {
                    allowablePressure = 18.6;
                }
                if (chainStep >= 31.75 && chainStep <= 38.1)
                {
                    allowablePressure = 16.3;
                }
                if (chainStep >= 44.45 && chainStep <= 50.8)
                {
                    allowablePressure = 0;
                }
            }
            else if (rpm2 <= 1200)
            {
                if (chainStep >= 12.7 && chainStep <= 15.87)
                {
                    allowablePressure = 20.6;
                }
                if (chainStep >= 19.05 && chainStep <= 25.04)
                {
                    allowablePressure = 17.2;
                }
                if (chainStep >= 31.75 && chainStep <= 38.1)
                {
                    allowablePressure = 14.7;
                }
                if (chainStep >= 44.45 && chainStep <= 50.8)
                {
                    allowablePressure = 0;
                }
            }
            Log("[p] = " + allowablePressure);
            Log("Коэффициент экспуатации (коэффициент нагрузки): Kэ = Kд * Kа * Kн * Kрег * Kсм * Kреж * Kт");
            Log("Kд = 1 - спокойная нагрузка;");
            Log("Kа = 1 - aw = (30 .. 50) * t;");
            Log("Kн = 1 - ψ≤45;");
            Log("Kрег = 1.25 - нерегулируемое положение звездочек;");
            Log("Kсм = 0.8 - непрерывное смазывание;");
            Log("Kреж = √(3)(Nсм) = 1 - редуктор работает одну смену;");
            Log("Kт = 1 - нормальные условия;");
            Log("Kэ = 1 * 1 * 1 * 1.25 * 0.8 * 1 * 1 = 1");
            Log("С учетом полученных результатов, расчитывается шаг цепи: t = 28 * √(3)((N * Kэ) / (ω1 * z1 * [p]));");
            chainStep = 28 * Math.Pow(((outPower * 1)/(angularVelocity2 * numberOfteethSprocket1 * allowablePressure)), 0.3333333);
            Log("t = " + chainStep + ";");
            Log("Полученное значение шага цеци округляется до ближайшего стандартного значения");
            double lengthMass = 6.2;
            double maxForceQ = 18000;
            double A = 39.6;
            double dp = 8.51;
            double Bvn = 7.75;
            if (chainStep <= 12.7)
            {
                chainStep = 12.7;
                lengthMass = 6.2;
                maxForceQ = 18000;
                dp = 8.51;
                Bvn = 7.75;
                A = 39.6;
            } else if (chainStep <= 15.875)
            {
                chainStep = 15.875;
                lengthMass = 8;
                maxForceQ = 23000;
                dp = 10.16;
                Bvn = 9.65;
                A = 51.5;
            } 
            else if (chainStep <= 19.05)
            {
                chainStep = 19.05;
                lengthMass = 15.2;
                maxForceQ = 25000;
                dp = 11.91;
                Bvn = 12.7;
                A = 106;
            }
            else if (chainStep <= 25.4)
            {
                chainStep = 25.4;
                lengthMass = 25.7;
                maxForceQ = 50000;
                dp = 15.88;
                Bvn = 15.88;
                A = 180;
            }
            else if (chainStep <= 31.75)
            {
                chainStep = 31.75;
                lengthMass = 37.3;
                maxForceQ = 70000;
                dp = 19.05;
                Bvn = 19.05;
                A = 262;
            }
            Log("t = " + chainStep + ";");
            Log("Угол поворота звеньев цепи на ведущей звездочке равен:");
            Log("φ = 360/ z1");
            double angleFi = 360 / numberOfteethSprocket1;
            Log("φ = " + angleFi + ";");
            Log("Делительные диаметры малой и большой звездочек определяется по выражениям: Dд1 = t / (sin(φ/2)); Dд2 = Dд1 * uвн");
            double chainGearDiameter1 = chainStep / Math.Sin((angleFi * 0.017) / 2);
            double chainGearDiameter2 = chainGearDiameter1 * chainGearRatio;
            Log("Dд1 = " + chainGearDiameter1 + " мм; Dд2 = " + chainGearDiameter2 + " мм;");
            Log("Межосевое расстояние в цепной передаче определяется по шагу цепи");
            Log("aw = (30 .. 50) * t");
            double centerToCenterDistance = 40 * chainStep;
            Log("aw = " + centerToCenterDistance + " мм;");
            Log("Длина цепи вычисляется по выражению:");
            Log("L = 2 * aw + 0.5 * (z1 + z2) * t + (((z1 + z2) / 2π)^2 * t^2) / aw");
            Log("Приняв обозначение At = aw / t, определяется число звеньев цепи:");
            Log("Lt = 2*At + 0.5 * (z1 + z2) + ((z1 + z2) / 2π)^2 / At");
            double chainLenghtT = 2 * 40 + ((numberOfteethSprocket1 + numberOfteethSprocket2) / 2) + (Math.Pow((numberOfteethSprocket1 + numberOfteethSprocket2) / (2 * Math.PI ), 2) / 40);
            Log("Lt = " + chainLenghtT + " ≈ " + Math.Round(chainLenghtT));
            chainLenghtT = Math.Round(chainLenghtT);
            Log("Решим уравнение для Lt относительно At");
            double zc = numberOfteethSprocket1 + numberOfteethSprocket2;
            double deltaD = zc / (2 * Math.PI);
            double At0 = (chainLenghtT - 0.5 * zc + Math.Sqrt((0.5 * zc - chainLenghtT) * (0.5 * zc - chainLenghtT) -  8 * deltaD * deltaD)) / 4;
            Log("At0 = " + At0);
            Log("Фактическое межосевое рассотяние aw = At0 * t");
            centerToCenterDistance = At0 * chainStep;
            Log("aw = " + centerToCenterDistance + " мм;");
            Log("Окружная сила на звездочках: Ft = 2 * 10^3 * T2 / Dд1");
            double forceTan = (2 * 1000 * torque2) / chainGearDiameter1;
            Log("Ft = " + forceTan);
            Log("Сила натяжения ведомой ветви: F2 = F0 + Fц, где F0 - натяжение от силы тяжести; Fц - натяжение от центробежных сил");
            double forceGravity = (lengthMass * 9.81 * centerToCenterDistance) / (8 * 0.01 * centerToCenterDistance);
            Log("F0 = (m1 * g * aw) / 8f, f = 0.001 * aw; F0 = " + forceGravity);
            Log("Натяжение цепи от действия центробежных сил: Fц = (m1 * V^2; V = z1 * t * n1) / (60 * 1000)");
            double forceNormal = lengthMass * Math.Pow(((rpm2 * chainStep * numberOfteethSprocket1) / (60 * 1000)), 2);
            Log("F0 = " + forceNormal);
            double force2 = forceGravity + forceNormal;
            Log("F2 = " + force2);
            Log("FΣ = Ft + 2 * F2 = " + (forceTan + 2 * force2));
            Log("\nПредварительный проверочный расчет:");
            Log("Определение статической разрушающей силы цепи: Fp = Ft * S, где S - коэффициент безопасности, При отсутствии коррозии S = 6 .. 10;");
            double forceBreak = forceTan * 8;
            Log("Fp = " + forceBreak);
            if(forceBreak < maxForceQ)
            {

                Log("Fp < Q; " + forceBreak + " < " + maxForceQ + " - верно!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Log("Fp >= Q; " + forceBreak + " >= " + maxForceQ + " - неверно!");
                Console.ForegroundColor = ConsoleColor.White;
            }
            Log("\nОсновной проверочный расчет:");
            Log("Давление в шарнирах p = (Kэ * Ft) / A <= [p], A определяется по табл 9.6");
            double jointPressure = forceTan / A;
            if (jointPressure < allowablePressure)
            {

                Log("p < [p]; " + jointPressure + " < " + allowablePressure + " - верно!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Log("p >= [p]; " + jointPressure + " >= " + allowablePressure + " - неверно!");
                Console.ForegroundColor = ConsoleColor.White;
            }
            Log("\nОсновные конструктивные размеры звездочек\n");
            Log("-Диаметры окружностей:");
            Log("выступов De = t * (K + ctg(180 / z))");
            double De1 = chainStep * (SelectKValue(numberOfteethSprocket1) + 1 / Math.Tan(Math.PI / numberOfteethSprocket1));
            double De2 = chainStep * (SelectKValue(numberOfteethSprocket2) + 1 /Math.Tan(Math.PI / numberOfteethSprocket2));
            Log("z1 = " + numberOfteethSprocket1 + " - K1 = " + SelectKValue(numberOfteethSprocket1) + ";\n De1 = " + De1);
            Log("z2 = " + numberOfteethSprocket2 + " - K2 = " + SelectKValue(numberOfteethSprocket2) + ";\n De2 = " + De2);
            Log("впадин Di = Dд - 2 * r");

            double y = 40.5 / numberOfteethSprocket1;
            double a = 57.5 / numberOfteethSprocket1;
            double b = 37 / numberOfteethSprocket1;
            double r = 0.5025 * dp + 0.05;
            double r1 = 0.8 * dp + r;
            double r2 = dp * (0.8 * Math.Cos(b * (Math.PI / 180)) + 1.24 * Math.Cos(y * (Math.PI / 180)) - 1.3025) - 0.05;

            double Di1 = chainGearDiameter1 - 2 * r;
            double Di2 = chainGearDiameter2 - 2 * r;
            Log("Di1 = " + Di1 + ";");
            Log("Di2 = " + Di2 + ";");
            Log("-Углы:");
            Log("половина угла зуба γ = 40.5 / z");
            Log("γ1 = " + y);
            Log("половина угла впадины α = 57.5 / z");
            Log("α1 = " + a);
            Log("сопряжения β = 37 / z");
            Log("β1 = " + b);
            Log("-Радиусы:");
            Log("Впадины зуба r = 0.5025 * dp + 0.05");
            Log("r = " + r);
            Log("сопряжения r1 = 0.8 * dp + r");
            Log("r1 = " + r1);
            Log("головки зуба r2 = dp * (0.8 * cos β + 1.24 * cos γ – 1.3025) – 0,05");
            Log("r2 = " + r2);
            Log("-Длина прямого участка fg = dp * (1.24 * sinγ – 0.8 * sinβ)");
            double fg = dp * (1.24 * Math.Sin(y * (Math.PI / 180)) - 0.8 * Math.Sin(b * (Math.PI / 180)));
            Log("fg = " + fg);
            Log("-Радиус закругления зуба r3 = 1.7 * dp");
            double r3 = 1.7 * dp;
            Log("r3 = " + r3);
            Log("-Расстояние О1О2 O1O2 = 1.24 * dp");
            double o1o2 = 1.24 * dp;
            Log("O1O2 = " + o1o2);
            Log("-Координаты:\n x1 = 0.8 * dp * sinα \n y1 = 0.8  * dp * cosα");
            double x1 = 0.8 * dp * Math.Sin(a * (Math.PI / 180));
            double y1 = 0.8 * dp * Math.Cos(a * (Math.PI / 180));
            double x2 = 1.24 * dp * Math.Cos(Math.PI / numberOfteethSprocket1);
            double y2 = 1.24 * dp * Math.Sin(Math.PI / numberOfteethSprocket1);
            Log("x1 = " + x1);
            Log("y1 = " + y1);
            Log("x2 = " + x2); 
            Log("y2 = " + y2);
            Log("-Координаты центра радиуса hr = 0.8 * dp");
            double hr = 0.8 * dp;
            Log("hr = " + hr);
            Log("-Ширина зуба \n однорядная цепь b = 0.93 * Bвн – 0.15");
            double bb = 0.93 * Bvn - 0.15;
            Log("b = " + bb);
            Log("-Толщина обода звездочки δa = 0.5t (для стали) ");
            double da = 0.5 * chainStep;
            Log("δa = " + da);
            Log("-Толщина диска звездочки δд = 0.5t (для стали) ");
            double dd = 0.5 * chainStep;
            Log("δд = " + dd);
        }
        public static double SelectKValue(double z)
        {
            double k = 0.56;
            if (z <= 11)
            {
                k = 0.58;
            }
            if (z > 11 && z <= 17)
            {
                k = 0.56;
            }
            else if (z > 17 && z <= 35)
            {
                k = 0.53;
            }
            else if (z > 35)
            {
                k = 0.5;
            }
            return k;
        }
        public static double[] SelectSize(double torque2, double bearingWidth)
        {
            double[] values = new double[2];
            double wallWidth = 20;
            double tableFValue = 35;
            bearingWidth = bearingWidth * 2;
            if (torque2 < 10 && bearingWidth >= 20 && bearingWidth <= 40)
            {
                wallWidth = Math.Round(bearingWidth);
                tableFValue = 42;
            }
            else if (torque2 < 20 && bearingWidth >= 25 && bearingWidth <= 45)
            {
                wallWidth = Math.Round(bearingWidth);
                tableFValue = 48;
            }
            else if (torque2 < 40 && bearingWidth >= 25 && bearingWidth <= 50)
            {
                wallWidth = Math.Round(bearingWidth);
                tableFValue = 55;
            }
            else if (torque2 < 60 && bearingWidth >= 25 && bearingWidth <= 55)
            {
                wallWidth = Math.Round(bearingWidth);
                tableFValue = 60;
            }
            else if (torque2 < 80 && bearingWidth >= 30 && bearingWidth <= 55)
            {
                wallWidth = Math.Round(bearingWidth);
                tableFValue = 65;
            }
            else if (torque2 < 100 && bearingWidth >= 30 && bearingWidth <= 60)
            {
                wallWidth = Math.Round(bearingWidth);
                tableFValue = 70;
            }
            else if (torque2 < 200 && bearingWidth >= 30 && bearingWidth <= 70)
            {
                wallWidth = Math.Round(bearingWidth);
                tableFValue = 75;
            }
            else if (torque2 < 400 && bearingWidth >= 40 && bearingWidth <= 80)
            {
                wallWidth = Math.Round(bearingWidth);
                tableFValue = 88;
            }
            values[0] = wallWidth;
            values[1] = tableFValue;
            return values;
        }
        public static double SelectYf(double numberOfTeeth)
        {
            double Yf = 4.28;
            if (numberOfTeeth <= 17)
                Yf = 4.28;
            else if (numberOfTeeth <= 20)
                Yf = 4.09;
            else if (numberOfTeeth <= 25)
                Yf = 3.9;
            else if (numberOfTeeth <= 30)
                Yf = 3.8;
            else if (numberOfTeeth <= 40)
                Yf = 3.7;
            else if (numberOfTeeth <= 50)
                Yf = 3.65;
            else if (numberOfTeeth <= 60)
                Yf = 3.62;
            else if (numberOfTeeth <= 80)
                Yf = 3.61;
            else if (numberOfTeeth <= 100)
                Yf = 3.6;
            else if (numberOfTeeth <= 150)
                Yf = 3.6;
            return Yf;
        }
        public static double roundWidth(double width)
        {
            if (width <= 16)
                width = 16;
            else if (width <= 18)
                width = 18;
            else if (width <= 20)
                width = 20;
            else if (width <= 22)
                width = 22;
            else if (width <= 25)
                width = 25;
            else if (width <= 28)
                width = 28;
            else if (width <= 32)
                width = 32;
            else if (width <= 36)
                width = 36;
            else if (width <= 40)
                width = 40;
            else if (width <= 45)
                width = 45;
            else if (width <= 50)
                width = 50;
            else if (width <= 56)
                width = 56;
            else if (width <= 63)
                width = 63;
            else if (width <= 71)
                width = 71;
            else if (width <= 80)
                width = 80;
            else if (width <= 90)
                width = 90;
            else if (width <= 100)
                width = 100;
            else if (width <= 110)
                width = 110;
            else if (width <= 125)
                width = 125;
            else if (width <= 140)
                width = 140;
            else if (width <= 160)
                width = 160;
            else if (width <= 180)
                width = 180;
            else if (width <= 200)
                width = 200;
            else if (width <= 220)
                width = 220;
            return width;
        }
        public static Bearing GetBearing(double shaftDiameter)
        {
            Bearing selectedBearing = new Bearing(-69, 420, 15, 0.5, "Amogus");
            Bearing[] bearings = new Bearing[12];

            bearings[0] = new Bearing(10, 30, 9, 1, "200");
            bearings[1] = new Bearing(12, 32, 10, 1, "201");
            bearings[2] = new Bearing(15, 35, 11, 1, "202");

            bearings[3] = new Bearing(17, 40, 12, 1, "203");
            bearings[4] = new Bearing(20, 47, 14, 1.5, "204");
            bearings[5] = new Bearing(25, 52, 15, 1.5, "205");

            bearings[6] = new Bearing(30, 62, 16, 1.5, "206");
            bearings[7] = new Bearing(35, 72, 17, 2, "207");
            bearings[8] = new Bearing(40, 80, 18, 2, "208");

            bearings[9] = new Bearing(45, 85, 19, 2, "209");
            bearings[10] = new Bearing(50, 90, 20, 2, "210");
            bearings[11] = new Bearing(55, 100, 21, 2.5, "211");

            int i = 0;
            while (bearings[i].diameterInner < shaftDiameter)
            {
                i = i + 1;
            }
            selectedBearing = bearings[i];
            return selectedBearing;
        }
        public static Motor GetMotor(double motorPower, double motorRPM)
        {
            Motor selectedMotor = new Motor("Error", -404, 69, 420);
            Motor[] motors = new Motor[49];
            motors[0] = new Motor("-", 0, 370, 0);
            motors[1] = new Motor("71А6", 915, 370, 19);
            motors[2] = new Motor("-", 0, 370, 0);
            motors[3] = new Motor("-", 0, 370, 0);

            motors[4] = new Motor("", 0, 550, 0);
            motors[5] = new Motor("71В6", 915, 550, 19);
            motors[6] = new Motor("71А4", 1357, 550, 19);
            motors[7] = new Motor("", 0, 550, 0);

            motors[8] = new Motor("90LA8", 705, 750, 24);
            motors[9] = new Motor("80А6", 920, 750, 22);
            motors[10] = new Motor("71В4", 1350, 750, 19);
            motors[11] = new Motor("71А2", 2820, 750, 19);

            motors[12] = new Motor("90LB8", 715, 1100, 24);
            motors[13] = new Motor("80В6", 920, 1100, 22);
            motors[14] = new Motor("80А4", 1395, 1100, 22);
            motors[15] = new Motor("71В2", 2805, 1100, 19);

            motors[16] = new Motor("100L8", 715, 1500, 28);
            motors[17] = new Motor("90L6", 920, 1500, 24);
            motors[18] = new Motor("80В4", 1395, 1500, 22);
            motors[19] = new Motor("80А2", 2805, 1500, 22);

            motors[20] = new Motor("112МА8", 709, 2200, 32);
            motors[21] = new Motor("100L6", 945, 2200, 28);
            motors[22] = new Motor("90L4", 1395, 2200, 24);
            motors[23] = new Motor("80В2", 2850, 2200, 22);

            motors[24] = new Motor("112МВ8", 709, 3000, 32);
            motors[25] = new Motor("112МА6", 950, 3000, 32);
            motors[26] = new Motor("100S4", 1410, 3000, 28);
            motors[27] = new Motor("90L2", 2850, 3000, 24);

            motors[28] = new Motor("132S8", 716, 4000, 38);
            motors[29] = new Motor("112МВ6", 950, 4000, 32);
            motors[30] = new Motor("100L", 1410, 4000, 28);
            motors[31] = new Motor("100S2", 2850, 4000, 28);

            motors[32] = new Motor("132М8", 712, 5500, 38);
            motors[33] = new Motor("132S6", 960, 5500, 38);
            motors[34] = new Motor("112М4", 1432, 5500, 322);
            motors[35] = new Motor("100L2", 2850, 5500, 28);

            motors[36] = new Motor("160S8", 727, 7500, 48);
            motors[37] = new Motor("132М6", 960, 7500, 38);
            motors[38] = new Motor("132S4", 1440, 7500, 38);
            motors[39] = new Motor("112М2", 2895, 7500, 32);

            motors[40] = new Motor("160М8", 727, 11000, 48);
            motors[41] = new Motor("160S6", 970, 11000, 48);
            motors[42] = new Motor("132М4", 1447, 11000, 38);
            motors[43] = new Motor("132М2", 2910, 11000, 38);

            motors[44] = new Motor("180М8", 731, 15000, 55);
            motors[45] = new Motor("160М6", 970, 15000, 48);
            motors[46] = new Motor("160S4", 1455, 15000, 48);
            motors[47] = new Motor("160S2", 2910, 15000, 42);

            int i = 0;
            while (motors[i].power < motorPower)
            {
                i++;
            }
            if (motors[i].power >= motorPower)
            {
                int indexRPM = 0;
                double[] rpms = new double[4];
                rpms[0] = Math.Abs(motors[i].rpm - motorRPM);
                rpms[1] = Math.Abs(motors[i + 1].rpm - motorRPM);
                rpms[2] = Math.Abs(motors[i + 2].rpm - motorRPM);
                rpms[3] = Math.Abs(motors[i + 3].rpm - motorRPM);

                for (int j = 0; j < 4; j++)
                {
                    if (rpms[j] == rpms.Min<double>())
                        indexRPM = j;
                }
                selectedMotor.power = motors[i + indexRPM].power;
                selectedMotor.rpm = motors[i + indexRPM].rpm;
                selectedMotor.type = motors[i + indexRPM].type;
                selectedMotor.diameterShaft = motors[i + indexRPM].diameterShaft;
            }

            return selectedMotor;
        }
        public static void Log(string logInfo)
        {
            Console.WriteLine(logInfo);
        }
    }
    class Motor
    {
        public double rpm;
        public double power;
        public double diameterShaft;
        public string type;
    
        public Motor(string thisType, double thisRPM, double thisPower, double thisDiameterShaft)
        {
            type = thisType;
            rpm = thisRPM;
            power = thisPower;
            diameterShaft = thisDiameterShaft;
        }
    }
    class Bearing
    {
        public double diameterInner;
        public double diameterOuter;
        public double width;
        public double chamfer;
        public string name;
        public Bearing(double thisDiameterInner, double thisDiameterOuter, double thisWidth, double thisChamfer, string thisName)
        {
            diameterInner = thisDiameterInner;
            diameterOuter = thisDiameterOuter;
            width = thisWidth;
            chamfer = thisChamfer;
            name = thisName;
        }
    }
}
