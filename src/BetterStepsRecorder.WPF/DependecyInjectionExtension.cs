using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace BetterStepsRecorder.WPF
{
    internal class DependecyInjectionExtension : MarkupExtension
    {
        public static Func<Type, object> Resolver { get; set; }
        public Type Type { get; set; }
        public override object ProvideValue(IServiceProvider serviceProvider) => Resolver?.Invoke(Type);
    }
}
