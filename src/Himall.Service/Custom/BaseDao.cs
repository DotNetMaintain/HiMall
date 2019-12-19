using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Data.Common;
using System.Data;
using Himall.Model;
using Himall.Core;
using MySql.Data.MySqlClient;
using Himall.Entity;

namespace Himall.Service
{
    public class BaseDao
    {
        public const string TablePropertyInfoKey = "Table-PropertyInfo-{0}";
        public virtual long Add(object model, DbTransaction dbTran = null)
        {
            using (DbConnection connection = new MySqlConnection(Connection.ConnectionString))
            {
                connection.Open();
                DbCommand dbCmd = connection.CreateCommand();
                if (dbTran != null)
                    dbCmd.Transaction = dbTran;

                //获取传入的数据类型
                Type modelType = model.GetType();
                // 通过类的标签获取表名
                TableNameAttribute attribute = (TableNameAttribute)TableNameAttribute.GetCustomAttribute(modelType, typeof(TableNameAttribute));
                // 获取这个类可实例化的属性
                PropertyInfo[] properties = Cache.Get<PropertyInfo[]>(string.Format(TablePropertyInfoKey, attribute.TableName));//
                if (properties == null)
                {
                    properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    Cache.Insert(string.Format(TablePropertyInfoKey, attribute.TableName), properties);
                }

                string filedNames = "";
                string fileValues = "";
                bool hasIncrementField = false;
                // 遍历类的属性
                foreach (PropertyInfo property in properties)
                {
                    // 
                    object[] fieldTypes = property.GetCustomAttributes(typeof(FieldTypeAttribute), true);
                    // 如果属性没有打标签则不需要插入到数据库
                    if (fieldTypes.Length == 0)
                        continue;
                    // 打的是自增的标签也不需要插入到数据库
                    if (IsIncrementField(fieldTypes))
                    {
                        hasIncrementField = true;
                        continue;
                    }
                    filedNames += property.Name + ",";

                    dbCmd.Parameters.Add(new MySqlParameter("@"+ property.Name, property.GetValue(model, null)));
                    //database.AddInParameter(dbCmd, property.Name, GetDbType(property.PropertyType), property.GetValue(model, null));
                }
                filedNames = filedNames.Remove(filedNames.Length - 1);
                fileValues = "@" + filedNames.Replace(",", ",@");

                dbCmd.CommandText = "INSERT INTO " + attribute.TableName + "(" + filedNames + ")VALUES(" + fileValues + ")";
                // 打了自增标签，则返回自动的值
                string bret = "";
                if (hasIncrementField)
                {
                    dbCmd.CommandText += "; SELECT @@IDENTITY";
                    bret = dbCmd.ExecuteScalar().ToString();
                }
                else
                {
                    bret = dbCmd.ExecuteNonQuery().ToString();
                }

                connection.Close();
                long Id = 0;
                long.TryParse(bret, out Id);
                return Id;
            }
        }

        public virtual bool Update(object model, DbTransaction dbTran = null)
        {
            using (DbConnection connection = new MySqlConnection(Connection.ConnectionString))
            {
                connection.Open();
                DbCommand dbCmd = connection.CreateCommand();
                if (dbTran != null)
                    dbCmd.Transaction = dbTran;
                //获取传入的数据类型
                Type modelType = model.GetType();
                // 通过类的标签获取表名
                TableNameAttribute attribute = (TableNameAttribute)TableNameAttribute.GetCustomAttribute(modelType, typeof(TableNameAttribute));
                // 获取这个类可实例化的属性
                PropertyInfo[] properties = Cache.Get<PropertyInfo[]>(string.Format(TablePropertyInfoKey, attribute.TableName));//
                if (properties == null)
                {
                    properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    Cache.Insert(string.Format(TablePropertyInfoKey, attribute.TableName), properties);
                }


                string updateFileds = "";
                string keyFileds = "";
                // 遍历类的属性
                foreach (PropertyInfo property in properties)
                {
                    // 获取属性所打的对应数据表字段的标签
                    object[] fieldTypes = property.GetCustomAttributes(typeof(FieldTypeAttribute), true);
                    // 如果属性没有打标签，则不需要修改到数据库
                    if (fieldTypes.Length == 0)
                        continue;

                    if (IsKeyField(fieldTypes))
                    {
                        keyFileds += property.Name + "=@" + property.Name + " AND ";
                    }
                    else
                    {
                        updateFileds += property.Name + "=@" + property.Name + ",";
                    }
                    dbCmd.Parameters.Add(new MySqlParameter("@" + property.Name, property.GetValue(model, null)));
                    //database.AddInParameter(dbCmd, property.Name, GetDbType(property.PropertyType), property.GetValue(model, null));
                }
                updateFileds = updateFileds.Remove(updateFileds.Length - 1);
                keyFileds = keyFileds.Remove(keyFileds.Length - 4);

                dbCmd.CommandText = "UPDATE " + attribute.TableName + " SET " + updateFileds + " WHERE " + keyFileds;

                return dbCmd.ExecuteNonQuery() >= 1;
            }
        }

        public virtual bool Delete<T>(long keyField, DbTransaction dbTran = null)
        {
            using (DbConnection connection = new MySqlConnection(Connection.ConnectionString))
            {
                connection.Open();
                DbCommand dbCmd = connection.CreateCommand();
                if (dbTran != null)
                    dbCmd.Transaction = dbTran;

                //获取传入的数据类型
                Type modelType = typeof(T);

                TableNameAttribute attribute = (TableNameAttribute)TableNameAttribute.GetCustomAttribute(modelType, typeof(TableNameAttribute));
                // 获取这个类可实例化的属性
                PropertyInfo[] properties = Cache.Get<PropertyInfo[]>(string.Format(TablePropertyInfoKey, attribute.TableName));//
                if (properties == null)
                {
                    properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    Cache.Insert(string.Format(TablePropertyInfoKey, attribute.TableName), properties);
                }

                string keyFileds = "";
                // 遍历类的属性
                foreach (PropertyInfo property in properties)
                {
                    // 获取属性所打的对应数据表字段的标签
                    object[] fieldTypes = property.GetCustomAttributes(typeof(FieldTypeAttribute), true);
                    // 只有主键标签作为删除的条件
                    if (fieldTypes.Length > 0 && IsKeyField(fieldTypes))
                    {
                        keyFileds += property.Name + "=@" + property.Name;

                        dbCmd.Parameters.Add(new MySqlParameter("@" + property.Name, keyField));
                        //database.AddInParameter(dbCmd, property.Name, GetDbType(property.PropertyType), keyField);

                        break;
                    }
                }

                dbCmd.CommandText = "DELETE FROM " + attribute.TableName + " WHERE " + keyFileds;
                return dbCmd.ExecuteNonQuery() >= 1;
            }
        }

        public virtual T Get<T>(long keyField) where T : new()
        {
            using (DbConnection connection = new MySqlConnection(Connection.ConnectionString))
            {
                connection.Open();
                DbCommand dbCmd = connection.CreateCommand();

                //获取传入的数据类型
                Type modelType = typeof(T), conversionType;
                T model = new T();
                // 通过类的标签获取表名
                TableNameAttribute attribute = (TableNameAttribute)TableNameAttribute.GetCustomAttribute(modelType, typeof(TableNameAttribute));
                // 获取这个类可实例化的属性
                PropertyInfo[] properties = Cache.Get<PropertyInfo[]>(string.Format(TablePropertyInfoKey, attribute.TableName));//
                if (properties == null)
                {
                    properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    Cache.Insert(string.Format(TablePropertyInfoKey, attribute.TableName), properties);
                }

                string keyFileds = "";
                // 遍历类的属性
                foreach (PropertyInfo property in properties)
                {
                    // 获取属性所打的对应数据表字段的标签
                    object[] fieldTypes = property.GetCustomAttributes(typeof(FieldTypeAttribute), true);
                    // 只有主键标签作为获取的条件
                    if (fieldTypes.Length > 0 && IsKeyField(fieldTypes))
                    {
                        keyFileds += property.Name + "=@" + property.Name;

                        dbCmd.Parameters.Add(new MySqlParameter("@" + property.Name, keyField));
                        //database.AddInParameter(dbCmd, property.Name, GetDbType(property.PropertyType), keyField);
                        break;
                    }
                }

                dbCmd.CommandText = "SELECT * FROM " + attribute.TableName + " WHERE " + keyFileds;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int count = reader.FieldCount;

                        for (int i = 0; i < count; i++)
                        {
                            if (reader[i] != DBNull.Value)
                            {
                                PropertyInfo pi = modelType.GetProperty(reader.GetName(i), BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                                if (pi != null)
                                {
                                    // 判断可为空的类型
                                    conversionType = pi.PropertyType;
                                    if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                                    {
                                        System.ComponentModel.NullableConverter nullableConverter = new System.ComponentModel.NullableConverter(conversionType);
                                        conversionType = nullableConverter.UnderlyingType;
                                    }

                                    // 如果是枚举
                                    if (conversionType.IsEnum)
                                    {
                                        object enumValue = Enum.ToObject(conversionType, reader[i]);
                                        pi.SetValue(model, enumValue, null);
                                    }
                                    else
                                    {
                                        var objectValue = Convert.ChangeType(reader[i], conversionType);
                                        if (conversionType.Equals(typeof(String)) && objectValue == null)
                                            objectValue = string.Empty;
                                        pi.SetValue(model, objectValue, null);

                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        return default(T);
                    }
                }

                return model;
            }
        }


        private bool IsIncrementField(object[] fieldTypes)
        {
            foreach (object fieldType in fieldTypes)
            {
                if (((FieldTypeAttribute)fieldType).FieldType == FieldType.IncrementField)
                    return true;
            }
            return false;
        }

        private bool IsKeyField(object[] fieldTypes)
        {
            foreach (object fieldType in fieldTypes)
            {
                if (((FieldTypeAttribute)fieldType).FieldType == FieldType.KeyField)
                    return true;
            }
            return false;
        }

        private DbType GetDbType(Type t)
        {
            DbType dbt;
            try
            {
                if (t.IsEnum)
                    dbt = DbType.Int32;
                else
                {
                    // 如果是可为空的值类型，同取原有值类型
                    if ((t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)))
                        t = t.GetGenericArguments()[0];

                    dbt = (DbType)Enum.Parse(typeof(DbType), t.Name);
                }
            }
            catch
            {
                dbt = DbType.Object;
            }
            return dbt;
        }

    }
}
