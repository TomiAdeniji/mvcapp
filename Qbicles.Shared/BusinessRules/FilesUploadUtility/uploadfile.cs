using System;
using System.Data;
using System.Reflection;

namespace Qbicles.BusinessRules.FilesUploadUtility
{
    public static class uploadfile
    {
        #region [ Methods - Calculated Balance ]
        /// <summary>
        /// Auto Calculate Balance
        /// </summary>
        /// <param name="openBalance">is last balance of Account upload</param>
        /// <param name="dtTransaction"> datatable read from upload file</param>
        /// <param name="reCalBalance"> true- re-calculate balance, false - don not re-calculate balance</param>
        /// <returns>DataTable after calculate</returns>
        public static DataTable AutoCalculateBalance(
            decimal openBalance, DataTable dtTransaction, bool isBalanceCol, bool isDebitCol, bool isCreditCol,bool reCalBalance)
        {
            if(dtTransaction.Rows.Count==0)
                return dtTransaction;
            decimal prevBalance = 0;
            decimal curBalance = 0;
            decimal openingBalance, firstBalance, firstDebit, firstCredit = 0;
            firstBalance = openBalance;
            if (isDebitCol)
            {
                decimal.TryParse(dtTransaction.Rows[0]["Debit"].ToString(), out firstDebit);
            }
            else
            {
                dtTransaction.Columns.Add("Debit", typeof(double)).SetOrdinal(dtTransaction.Columns.Count - 1);
                firstDebit = 0;
            }
            if (isCreditCol)
            {
                decimal.TryParse(dtTransaction.Rows[0]["Credit"].ToString(), out firstCredit);
            }
            else
            {
                dtTransaction.Columns.Add("Credit", typeof(double)).SetOrdinal(dtTransaction.Columns.Count - 1);
                firstCredit = 0;
            }
            //get opening balance
            if (isBalanceCol)
            {
                decimal.TryParse(dtTransaction.Rows[0]["Balance"].ToString(), out firstBalance);
                openingBalance = firstBalance - firstDebit + firstCredit;
            }
            else
            {
                dtTransaction.Columns.Add("Balance" , typeof(double)).SetOrdinal(dtTransaction.Columns.Count - 1);
                openingBalance = openBalance;
            }
            if(!reCalBalance)
                return dtTransaction;

            try
            {
                prevBalance = openingBalance;
                foreach (DataRow row in dtTransaction.Rows)
                {
                    if (row == null)
                    {
                        continue;
                    }

                    bool isDebit = false;
                    
                    decimal? amount = 0;
                    if ((string)row["Debit"].ToString() == "0" || string.IsNullOrWhiteSpace(row["Debit"].ToString()))
                    {
                        isDebit = false;
                        amount = (HelperClass.Converter.Obj2DecimalNull(row["Credit"]));
                        if (amount == null)
                            amount = 0;
                      
                    }
                    else
                    {
                        isDebit = true;
                        amount = HelperClass.Converter.Obj2DecimalNull(row["Debit"]);
                        
                        if (amount == null)
                            amount = 0;
                    }

                    if (isDebit)
                    {
                        curBalance = prevBalance + (decimal)amount;
                    }
                    else
                    {
                        curBalance = prevBalance - (decimal)amount;
                    }

                    row["Balance"] = curBalance.ToString();

                    prevBalance = curBalance;

                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }
            return dtTransaction;

        }
        #endregion
    }
}