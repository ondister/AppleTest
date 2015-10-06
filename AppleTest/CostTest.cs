using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ostis.Sctp;
using Ostis.Sctp.Arguments;
using Ostis.Sctp.Commands;
using Ostis.Sctp.Responses;


namespace AppleTest
{
    internal class CostTest
    {
        private readonly Identifier bittersweet_apple = "bittersweet_apple";
        private readonly Identifier class_apple = "class_apple";
        private readonly Identifier nrel_price = "nrel_price";

        public Identifier Nrel_price
        {
            get { return nrel_price; }
        }

        private readonly Identifier nrel_cost = "nrel_cost";

        public Identifier Nrel_cost
        {
            get { return nrel_cost; }
        }

        private readonly Identifier nrel_quantity = "nrel_quantity";

        public Identifier Nrel_quantity
        {
            get { return nrel_quantity; }
        } 


        SctpClient client;
        public CostTest(string address, int port)
        {
            client = new SctpClient(address, port);
            client.Connect();
        }

        public List<ScAddress> FindApples()
        {
            List<ScAddress> apples = new List<ScAddress>();
            //ищем адресс класса яблоко
            var classAppleAddress = this.FindElementAddress(class_apple);
            //ищем все экземпляры класса яблоко. Вот здесь обычно очень нужно находить подмаски масок элементов и сравнивать их
            var template = new ConstructionTemplate(classAppleAddress, ElementType.PositiveConstantPermanentAccessArc, ElementType.ConstantNode);
            var cmdFindApples = new IterateElementsCommand(template);
            var rspIterateAppels = (IterateElementsResponse)client.Send(cmdFindApples);

            //и при итерации добавляем в коллекцию адреса яблок (у нас в примере один, но суть можно уловить)
            foreach (var construction in rspIterateAppels.Constructions)
            {
                apples.Add(construction[2]);
            }

            return apples;
        }

        public void CreateCost(ScAddress node)
        {
            //ищем адресс узла стоимость
            var costAddress = this.FindElementAddress(nrel_cost);

            // ищем адрес узла цена
            var priceAddress = this.FindElementAddress(nrel_price);
            //ищем значение узла по предикату
            double price = this.FindValueByPredicate(node, priceAddress);

            //ищем адрес узла количество
            var quantityAddress = this.FindElementAddress(nrel_quantity);
            //ищем значение узла по предикату
            double quantity = this.FindValueByPredicate(node, quantityAddress);

            //если все найдено, делаем стоимость
            if (price != double.NaN && quantity != double.NaN)
            {
              
                //создаем ссылку, и задаем ей значение в виде произведения
                var cmdCreateLink = new CreateLinkCommand();
                var rspCreateLink = (CreateLinkResponse)client.Send(cmdCreateLink);

                var cmdSetValue = new SetLinkContentCommand(rspCreateLink.CreatedLinkAddress, new LinkContent(price * quantity));
                var rspSetValue = (SetLinkContentResponse)client.Send(cmdSetValue);

                // соединяем все дугами
                var commonArcAdr = this.CreateArcCommand(ElementType.ConstantCommonArc, node, rspCreateLink.CreatedLinkAddress);
                this.CreateArcCommand(ElementType.PositiveConstantPermanentAccessArc, costAddress, commonArcAdr);

            }

        }

        public double GetCost(ScAddress node)
        {
            var costAddress = this.FindElementAddress(nrel_cost);
            return this.FindValueByPredicate(node, costAddress);
        }


        private ScAddress CreateArcCommand(ElementType arcType, ScAddress beginElement, ScAddress endElement)
        {
            var cmdCreateArc = new CreateArcCommand(arcType, beginElement, endElement);
            var rspCreateArc = (CreateArcResponse)client.Send(cmdCreateArc);
            return rspCreateArc.CreatedArcAddress;
        }
        private ScAddress FindElementAddress(Identifier identifier)
        {
            ScAddress scaddress = ScAddress.Unknown;
            var cmdFindElement = new FindElementCommand(identifier);
            var rspFindElement = (FindElementResponse)client.Send(cmdFindElement);
            if (rspFindElement.Header.ReturnCode == ReturnCode.Successfull)
            {
                scaddress = rspFindElement.FoundAddress;
            }
            return scaddress;
        }

        /// <summary>
        /// Функция для поиска значения ссылки по отношению, например в констукции bittersweet_apple => nrel_price:[5,5];
        /// </summary>
        /// <param name="node">это адрес узла, например,  bittersweet_apple у которого есть ссылка, например [5,5]</param>
        /// <param name="predicate">это отношение, например nrel_price, которое поясняет, что это за ссылка</param>
        /// <returns></returns>
       private double FindValueByPredicate(ScAddress node, ScAddress predicate)
        {
            double value = double.NaN;
            //итерируем конструкцию
            var template = new ConstructionTemplate(node, ElementType.ConstantCommonArc, ElementType.Link, ElementType.PositiveConstantPermanentAccessArc, predicate);
            var cmdIterateElements = new IterateElementsCommand(template);
            var rspIterateElements = (IterateElementsResponse)client.Send(cmdIterateElements);

            //если число конструкций равно 1, то ищем значение ссылки

            if (rspIterateElements.Constructions.Count() == 1)
            {
                var cmdGetValue = new GetLinkContentCommand(rspIterateElements.Constructions[0][2]);
                var rspGetValue = (GetLinkContentResponse)client.Send(cmdGetValue);
                value = LinkContent.ToDouble(rspGetValue.LinkContent);

            }
            if (predicate.Offset == 2087)
            {
                int i = 0;
                i = 1;
            }
            return value;
        }

      public double FindValueByPredicate(ScAddress node, Identifier predicate)
       {
           return this.FindValueByPredicate(node, this.FindElementAddress(predicate));
       }



    }
}
