using System;

namespace Himall.Model
{
    ///<summary>
    /// 指示实体对象中对应数据数的表名
    ///</summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class TableNameAttribute : Attribute
    {
        private string tableName;

        /// <summary>
        /// 指示实体对象中对应数据数的表名
        /// </summary>
        /// <param name="tableName"></param>
        public TableNameAttribute(string tableName)
        {
            this.tableName = tableName;
        }

        /// <summary>
        /// 获取当前对象对应的表名
        /// </summary>
        public string TableName
        {
            get { return tableName; }
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class FieldTypeAttribute: Attribute
    {
        private FieldType fieldType;

        public FieldType FieldType
        {
            get { return fieldType; }
        }

        public FieldTypeAttribute(FieldType fieldType)
        {
            this.fieldType = fieldType;
        }
    }

    public enum FieldType
    {
        /// <summary>
        /// 一般字段
        /// </summary>
        CommonField = 1,

        /// <summary>
        /// 主建
        /// </summary>
        KeyField = 2,

        /// <summary>
        /// 自增
        /// </summary>
        IncrementField = 3,
    }
}