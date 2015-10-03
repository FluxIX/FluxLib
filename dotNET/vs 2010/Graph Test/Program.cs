using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Flux.Collections.Generic.Graph.Test
{
   class Program
   {
      [STAThread]
      static void Main( string[] args )
      {
         string[] my_args = { Assembly.GetExecutingAssembly().Location };

         int returnCode = NUnit.ConsoleRunner.Runner.Main( my_args );

         if( returnCode != 0 )
            Console.Beep();
      }
   }
}
