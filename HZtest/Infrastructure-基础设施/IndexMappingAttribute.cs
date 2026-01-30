using System;
using System.Collections.Generic;
using System.Text;

namespace HZtest.Infrastructure
{
    // 定义特性：标记属性对应的API返回索引
    [AttributeUsage(AttributeTargets.Property)]
    public class IndexMappingAttribute : Attribute
    {
        public int Index { get; }
        public string Description { get; }

        public IndexMappingAttribute(int index, string description = "")
        {
            Index = index;
            Description = description;
        }
    }

}
