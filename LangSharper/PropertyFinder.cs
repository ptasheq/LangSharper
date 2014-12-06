using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LangSharper
{
    public class PropertyFinder
    {
        IDictionary _dict; 
        static PropertyFinder instance;

        PropertyFinder() : this(Application.Current.Properties)
        {
        }

        PropertyFinder(IDictionary dict)
        {
            _dict = dict;
        }

        static public PropertyFinder Instance
        {
            get { return instance ?? (instance = new PropertyFinder()); }
        }

        public object GetResource(string key)
        {
            return _dict[key];
        }

        public IDictionary Resource { get { return _dict; } }

        static public void CreateInstance(IDictionary dict)
        {
            instance = new PropertyFinder(dict);
        }
    }
}
