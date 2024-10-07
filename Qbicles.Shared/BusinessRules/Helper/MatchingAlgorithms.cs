using Qbicles.BusinessRules;
using static Qbicles.BusinessRules.Enums;

namespace CleanBooksCloud.Helper
{
    public class MatchingAlgorithms
    {
        /// <summary>
        /// This is the algorithm to check if the amounts in two transactions match
        /// </summary>
        /// <param name="TranA"></param>
        /// <param name="TranB"></param>
        /// <param name="transactionMatchingTypeId"></param>
        /// <returns></returns>
        public static bool AmountsMatch(transactionsMatchingModel TranA, transactionsMatchingModel TranB, int transactionMatchingTypeId, decimal VarianceA, decimal VarianceB, ref bool IsAmount1VarianceUsed, ref bool IsAmount2VarianceUsed)
        {
            bool DoAmountsMatch = false;
            decimal _VarianceA = 0, _VarianceB = 0;
            VarianceA = VarianceA / (decimal)100.0;
            VarianceB = VarianceB / (decimal)100.0;
            //If TranA is a debit then
            if (TranA.Debit != null)
            {
                _VarianceA = (TranA.Debit ?? 0) * VarianceA;
                switch (transactionMatchingTypeId)
                {
                    case (int)tranMatchingType.DebitToCredit://if transactionmatchingtype = Debit to Credit then
                        //If TranA.Debit = TranB.Credit then
                        _VarianceB = (TranB.Credit ?? 0) * VarianceB;
                        if (TranA.Debit == TranB.Credit)
                            DoAmountsMatch = true;
                        else if ((TranA.Debit - _VarianceA) <= TranB.Credit && (TranA.Debit + _VarianceA) >= TranB.Credit)
                        {
                            DoAmountsMatch = true;
                            IsAmount1VarianceUsed = true;
                        }
                        else if ((TranB.Credit - _VarianceB) <= TranA.Debit && (TranB.Credit + _VarianceB) >= TranA.Debit)
                        {
                            DoAmountsMatch = true;
                            IsAmount2VarianceUsed = true;
                        }
                        break;
                    case (int)tranMatchingType.DebitToDebit://Else if transactionmatchingtype = Debit to Debit then
                        //If TranA.Debit = TranB.Debit then
                        _VarianceB = (TranB.Debit ?? 0) * VarianceB;
                        if (TranA.Debit == TranB.Debit)
                            DoAmountsMatch = true;
                        else if ((TranA.Debit - _VarianceA) <= TranB.Debit && (TranA.Debit + _VarianceA) >= TranB.Debit)
                        {
                            DoAmountsMatch = true;
                            IsAmount1VarianceUsed = true;
                        }
                        else if ((TranB.Debit - _VarianceB) <= TranA.Debit && (TranB.Debit + _VarianceB) >= TranA.Debit)
                        {
                            DoAmountsMatch = true;
                            IsAmount2VarianceUsed = true;
                        }
                        break;
                }
            }
            else if (TranA.Credit != null)//Else if TranA is a credit then
            { 
                _VarianceA = (TranA.Credit??0) * VarianceA;
                switch (transactionMatchingTypeId)
                {
                    case (int)tranMatchingType.DebitToCredit://if transactionmatchingtype = Debit to Credit then
                        //If TranA.Credit = TranB.Debit then
                        _VarianceB = (TranB.Debit??0) * VarianceB;
                        if (TranA.Credit == TranB.Debit)
                            DoAmountsMatch = true;
                        else if((TranA.Credit - _VarianceA) <= TranB.Debit && (TranA.Credit + _VarianceA) >= TranB.Debit)
                        {
                            DoAmountsMatch = true;
                        }
                        else if( (TranB.Debit - _VarianceB) <= TranA.Credit && (TranB.Debit + _VarianceB) >= TranA.Credit)
                        {
                            DoAmountsMatch = true;
                        }
                        break;
                    case (int)tranMatchingType.DebitToDebit://Else if transactionmatchingtype = Debit to Debit then
                        //If TranA.Credit = TranB.Credit then
                        _VarianceB = (TranB.Credit??0) * VarianceB;
                        if (TranA.Credit == TranB.Credit)
                            DoAmountsMatch = true;
                        else if((TranA.Credit - _VarianceA) <= TranB.Credit && (TranA.Credit + _VarianceA) >= TranB.Credit)
                        {
                            DoAmountsMatch = true;
                        }
                        else if( (TranB.Credit - VarianceB) <= TranA.Credit  && (TranB.Credit + VarianceB) >= TranA.Credit)
                        {
                            DoAmountsMatch = true;
                        }
                        break;
                }
            }
            return DoAmountsMatch;
        }
        /// <summary>
        /// In this matching method we attempt to match two transactions by Reference
        /// </summary>
        /// <param name="TranA"></param>
        /// <param name="TranB"></param>
        /// <returns></returns>
        public static bool Reference(transactionsMatchingModel TranA, transactionsMatchingModel TranB)
        {
            bool DoReferenceMatch = false;
            //Check if the references match

            if (TranA.Reference == null || TranB.Reference == null)//if TranA.Reference = NULL OR TranB.Reference = NULL then NO Match
                DoReferenceMatch = false;
            else if (TranA.Reference.Trim().ToUpper() == TranB.Reference.Trim().ToUpper())//If TranA.Reference.Trim.ToUpper = TranB.Reference.Trim.ToUpper then we have a match.
                DoReferenceMatch = true;
            else if (PatternMatchingManager.CheckReferenceWithReference(TranA.Reference, TranB.Reference))//If PatternMatchingManager.CheckReferenceWithReference(TranA.Reference, TranB.Reference) is TRUE then we have a match
                DoReferenceMatch = true;
            return DoReferenceMatch;
        }
        /// <summary>
        /// In this matching method we attempt to match two transactions by Reference and Reference1
        /// </summary>
        /// <param name="TranA"></param>
        /// <param name="TranB"></param>
        /// <returns></returns>
        public static bool ReferenceToReference1(transactionsMatchingModel TranA, transactionsMatchingModel TranB)
        {
            if (TranA.Reference != null && TranB.Reference1 != null)
            {
                if (PatternMatchingManager.CheckReferenceWithReference(TranA.Reference, TranB.Reference1))
                    return true;
            }

            if (TranA.Reference1 != null && TranB.Reference != null)//If TranA.Reference1 is not Null AND If TranB.Reference is not Null then
            {
                if (PatternMatchingManager.CheckReferenceWithReference(TranA.Reference1, TranB.Reference))
                    return true;
            }

            if (TranA.Reference1 != null && TranB.Reference1 != null)
            {
                if (PatternMatchingManager.CheckReferenceWithReference(TranA.Reference1, TranB.Reference1))
                    return true;
            }

            return false;
        }
        /// <summary>
        /// In this matching method we attempt to match two transactions by Reference and Description
        /// </summary>
        /// <param name="TranA"></param>
        /// <param name="TranB"></param>
        /// <returns></returns>
        public static bool ReferenceToDescription(transactionsMatchingModel TranA, transactionsMatchingModel TranB)
        {
            if (TranA.Reference != null && TranB.Description != null)
            {
                if (PatternMatchingManager.CheckReferenceWithDescription(TranA.Reference, TranB.Description))
                    return true;
            }


            if (TranA.Description != null && TranB.Reference != null)
            {
                if (PatternMatchingManager.CheckReferenceWithDescription(TranB.Reference, TranA.Description))
                    return true;
            }

            return false;
        }
        /// <summary>
        /// In this matching method we attempt to match two transactions by Description
        /// </summary>
        /// <param name="TranA"></param>
        /// <param name="TranB"></param>
        /// <returns></returns>
        public static bool Description(transactionsMatchingModel TranA, transactionsMatchingModel TranB)
        {
            if (TranA.Description == null || TranB.Description == null)
                return false;

            if (TranA.Description.Trim().ToUpper() == TranB.Description.Trim().ToUpper())
                return true;

            if (PatternMatchingManager.CheckDescriptionWithDescription(TranA.Description, TranB.Description))
                return true;

            return false;
        }
    }
}