
using System;
using System.Data;
using System.Text;
using System.Collections;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.Util {

    public class TraceViewUtil {

        public static String ConvertToPerformanceView(DateTime before, DateTime after) {
            TimeSpan ts = after - before;
            if (ts.TotalMilliseconds < 0 ) {
                // no exception because this method is basically for logging
                return ts.TotalMilliseconds.ToString();
            }
            int min = (((ts.Days * 24) + ts.Hours) * 60) + ts.Minutes;
            String minExp = (min >= 10 ? min.ToString() : min.ToString("00"));
            return string.Format("{0}m{1}s{2}ms", minExp, ts.Seconds.ToString("00"), ts.Milliseconds.ToString("000"));
        }

        public static String ConvertObjectArrayToStringView(Object[] objArray) {
            if (objArray == null) {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < objArray.Length; i++) {
                if (i == 0) {
                    sb.Append(objArray[i]);
                } else {
                    sb.Append(", ").Append(objArray[i]);
                }
            }
            return sb.ToString();
        }
        
        public static string ToStringView(object target) {
            if (target == null) {
                return "null";
            }
            string ret;
            if (target is IDictionary) {
                ret = ToStringView(target as IDictionary);
            } else if (target is IList) {
                ret = ToStringView(target as IList);
            } else if (target is DataSet) {
                ret = ToStringView(target as DataSet);
            } else if (target is DataTable) {
                ret = ToStringView(target as DataTable);
            } else {
                ret = target.ToString();
            }
            return ret;
        }

        public static string ToStringView(IDictionary target) {
            if (target == null) {
                return "null";
            }

            StringBuilder buf = new StringBuilder();

            buf.Append("{");
            foreach (object key in target.Keys) {
                buf.AppendFormat("{0}={1}, ", ToStringView(key), ToStringView(target[key]));
            }
            if (target.Keys.Count > 0) {
                buf.Length -= 2;
            }
            buf.Append("}");

            return buf.ToString();
        }

        public static string ToStringView(IList target) {
            if (target == null) {
                return "null";
            }

            StringBuilder buf = new StringBuilder();
            buf.Append("{");
            foreach (object o in target) {
                buf.Append(ToStringView(o));
                buf.Append(", ");
            }
            if (target.Count > 0) {
                buf.Length -= 2;
            }
            buf.Append("}");

            return buf.ToString();
        }

        public static string ToStringView(DataSet target) {
            if (target == null) {
                return "null";
            }

            StringBuilder buf = new StringBuilder();

            foreach (DataTable table in target.Tables) {
                buf.Append(ToStringView(table));
                buf.Append(Environment.NewLine);
            }

            return buf.ToString();
        }

        public static string ToStringView(DataTable target) {
            if (target == null) {
                return "null";
            }

            StringBuilder buf = new StringBuilder();

            buf.AppendFormat(target.TableName);
            buf.Append(Environment.NewLine);

            DataRowCollection tableRows = target.Rows;
            DataColumnCollection tableColumns = target.Columns;
            for (int ctrRow = 0; ctrRow < tableRows.Count; ctrRow++) {
                DataRow row = tableRows[ctrRow];
                buf.AppendFormat("Row #{0}-", ctrRow + 1);
                buf.Append(Environment.NewLine);
                object[] rowItems = row.ItemArray;

                for (int ctrColumn = 0; ctrColumn < tableColumns.Count; ctrColumn++) {
                    DataColumn column = tableColumns[ctrColumn];
                    buf.AppendFormat("\t{0}: {1}", column.ColumnName, rowItems[ctrColumn]);
                    buf.Append(Environment.NewLine);
                }
            }
            buf.Append(Environment.NewLine);

            return buf.ToString();
        }
        
    }
}
