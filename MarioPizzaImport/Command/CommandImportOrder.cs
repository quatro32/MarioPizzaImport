using MarioPizzaImport.Import;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarioPizzaImport.Command
{
    class CommandImportOrder : CommandImport<order>
    {
        public override string GetCommandName()
        {
            return "import-order";
        }

        protected override Importer<order> CreateImporter(dbi298845_prangersEntities database, countrycode countrycode)
        {
            return new OrderImporter(database, countrycode);
        }
    }
}
