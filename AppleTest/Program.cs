using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ostis.Sctp;
using Ostis.Sctp.Arguments;
using Ostis.Sctp.Tools;
namespace AppleTest
{
    class Program
    {
        static void Main(string[] args)
        {

            //Итак, очень простой пример: В базе создадим узел Горькосладкое яблоко и с известной ценой и количеством
            //Затем напишем простого агента, который будет создавать стоимость для любого узла, у которого есть цена и количество (собственно не только для яблок).
            //Можно сделать более универсального агента, который будет знать по каким правилам формировать стоимость, но это позже, после усвоения языка scs и основных концепций
            //я НЕ буду использовать врапера Tools, чтобы его необходимость осозналась
            // также я не буду использовать проверку на ошибки
            //Вообще вся логика работы с базой знаний строится на работе с конечным количеством отношений и классов.
            //Можно заметить, что я создал в папке scs класс яблоко и отношения цена, количество и стоимость, 
            //а так же горько-сладкое яблоко, как экземпляр класса яблоко и задал его цену и количество
            //Необходимо с помощью утилиты sc-builder.exe транслировать их в базу
            //все остальные объяснения в классе  CostTest

            CostTest costTest = new CostTest("127.0.0.1", SctpProtocol.DefaultPortNumber);
            KnowledgeBase kb = new KnowledgeBase("127.0.0.1", SctpProtocol.DefaultPortNumber);
           // Console.WriteLine(kb.Nodes["nrel_cost"].ScAddress.Segment);

            // Ищем все яблоки, собстенно оно у нас одно
            var applesAddresses = costTest.FindApples();
            //для всех яблок создаем стоимость
            foreach (var appleAdr in applesAddresses)
            {
                double cost=costTest.FindValueByPredicate(appleAdr, costTest.Nrel_cost);
                if (double.IsNaN(cost))
                {
                    costTest.CreateCost(appleAdr);
                }
                //ну и тут же проверяем, создалось ли
                Console.WriteLine("Цена: {0}", costTest.FindValueByPredicate(appleAdr, costTest.Nrel_price));
                Console.WriteLine("Количество: {0}", costTest.FindValueByPredicate(appleAdr, costTest.Nrel_quantity));
                Console.WriteLine("Стоимость: {0}", costTest.FindValueByPredicate(appleAdr, costTest.Nrel_cost));
            }
            
          Console.ReadKey();
        }
    }
}
