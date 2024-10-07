using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CleanBooksCloud.Helper
{
    public class PatternMatchingManager
    {
        #region [ Fields ]
        static char[] splitChars = new char[] { ' ', '.', '@', '-', ':', '/', '#', '&', '\\', ',' };
        static char[] refColShortSplitChars = new char[] { '/', '\\' };
        static char[] refColLongSplitChars = new char[] { '-', ':', ' ', '/', '\\' };
        static char[] descColSplitChars = new char[] { ' ', '.', '@', '-', ':', '/', '#', '&', '/' };
        #endregion

        public static string Soundex(string data)
        {
            StringBuilder result = new StringBuilder();
            if (data != null && data.Length > 0)
            {
                string previousCode = "", currentCode = "", currentLetter = "";
                result.Append(data.Substring(0, 1));
                for (int i = 1; i < data.Length; i++)
                {
                    currentLetter = data.Substring(i, 1).ToLower();

                    if ("BFPV".IndexOf(currentLetter) > -1)
                        currentCode = "1";
                    else if ("CGJKQSXZ".IndexOf(currentLetter) > -1)
                        currentCode = "2";
                    else if ("DT".IndexOf(currentLetter) > -1)
                        currentCode = "3";
                    else if (currentLetter == "L")
                        currentCode = "4";
                    else if ("MN".IndexOf(currentLetter) > -1)
                        currentCode = "5";
                    else if (currentLetter == "R")
                        currentCode = "6";

                    if (currentCode != previousCode)
                        result.Append(currentCode);

                    if (result.Length == 4) break;

                    if (currentCode != "")
                        previousCode = currentCode;
                }
            }

            if (result.Length < 4)
                result.Append(new String('0', 4 - result.Length));

            return result.ToString().ToUpper();
        }

        public static int Difference(string data1, string data2)
        {
            int result = 0;
            string soundex1 = Soundex(data1);
            string soundex2 = Soundex(data2);

            if (data1.Contains(data2) || data2.Contains(data1))
            {
                result = 5;
            }
            else if (soundex1 == soundex2)
                result = 5;
            else
            {
                string sub1 = soundex1.Substring(1, 3);
                string sub2 = soundex1.Substring(2, 2);
                string sub3 = soundex1.Substring(1, 2);
                string sub4 = soundex1.Substring(1, 1);
                string sub5 = soundex1.Substring(2, 1);
                string sub6 = soundex1.Substring(3, 1);

                if (soundex2.IndexOf(sub1) > -1)
                    result = 3;
                else if (soundex2.IndexOf(sub2) > -1)
                    result = 2;
                else if (soundex2.IndexOf(sub3) > -1)
                    result = 2;
                else
                {
                    if (soundex2.IndexOf(sub4) > -1)
                        result++;
                    if (soundex2.IndexOf(sub5) > -1)
                        result++;
                    if (soundex2.IndexOf(sub6) > -1)
                        result++;
                }

                if (soundex1.Substring(0, 1) == soundex2.Substring(0, 1))
                    result++;
            }
            return (result == 0) ? 1 : result;
        }

        public static bool MatchTellerNoInDescription(string tellerNo, string description)
        {
            return description.Contains(tellerNo);
        }

        public static int AproxMatchNameInDescription(string name, string description)
        {
            int totalRank = 0;
            int singleDigitRankCount = 0;
            int rankCount = 0;
            name = name.ToUpper().Trim();
            description = description.ToUpper().Trim();

            string[] nameParts = name.Split(new char[] { ' ', '.', '&', '/' });
            string[] descParts = description.Split(new char[] { ' ', '.', '@', '-', ':', '/', '#', '&', '/' });
            foreach (string namePart in nameParts)
            {
                string copyNamePart = namePart.Trim();
                if (copyNamePart.Length <= 1)
                {
                    foreach (string descPart in descParts)
                    {
                        string copyDescPart = descPart.Trim();
                        if (copyNamePart.Equals(copyDescPart))
                        {
                            singleDigitRankCount++;
                        }
                    }
                    continue;
                }
                foreach (string descPart in descParts)
                {
                    string copyDescPart = descPart.Trim();
                    if (copyDescPart.Length <= 1)
                    {
                        continue;
                    }
                    int rank = Difference(copyNamePart, copyDescPart);
                    if (rank > 4)
                    {
                        totalRank += rank;
                        rankCount++;
                        break;
                    }
                }
            }

            int totalRankCount = singleDigitRankCount + rankCount;
            if (totalRankCount != nameParts.Length)
            {
                if (rankCount == 0)
                {
                    totalRank = 0;
                }
            }
            return totalRank;
        }

        public static int AproxMatchDescriptionInDescription(string description1, string description)
        {
            int totalRank = 0;
            description1 = description1.ToUpper();
            description = description.ToUpper();

            string[] nameParts = description1.Split(new char[] { ' ', '.', '@', '-', ':', '/', '#' });
            string[] descParts = description.Split(new char[] { ' ', '.', '@', '-', ':', '/', '#' });
            foreach (string namePart in nameParts)
            {
                if (namePart.Length <= 1)
                {
                    continue;
                }
                foreach (string descPart in descParts)
                {
                    if (descPart.Length <= 1)
                    {
                        continue;
                    }
                    int rank = Difference(namePart, descPart);
                    if (rank > 4)
                    {
                        totalRank += rank;
                        break;
                    }
                }
            }

            return totalRank;
        }

        public static bool PerfectMatchNameInDescription(string name, string description)
        {
            name = name.ToUpper();
            description = description.ToUpper();
            string[] nameParts = name.Split(new char[] { ' ', '.', '&', '/' });
            string[] descParts = description.Split(new char[] { ' ', '.', '@', '-', ':', '/', '#', '&', '/' });
            int noOfMatches = 0;
            foreach (string namepart in nameParts)
            {
                string copyNamePart = namepart.Trim();
                foreach (string descPart in descParts)
                {
                    string copyDescPart = descPart.Trim();
                    if (namepart.Equals(descPart))
                    {
                        noOfMatches++;
                        break;
                    }
                }
            }

            if (noOfMatches == nameParts.Length)
            {
                return true;
            }

            return false;
        }
        public static bool PerfectMatchCBDescInBKSDesc(string cbDescription, string bksDescription)
        {
            cbDescription = cbDescription.ToUpper();
            bksDescription = bksDescription.ToUpper();

            if (cbDescription.Length > 2 && bksDescription.Length > 2)
            {
                if (cbDescription == bksDescription)
                {
                    return true;
                }
            }

            string[] nameParts = cbDescription.Split(new char[] { ' ', '.', '@', '-', ':', '/', '#', '&', '\\', ',' });
            string[] descParts = bksDescription.Split(new char[] { ' ', '.', '@', '-', ':', '/', '#', '&', '\\', ',' });

            List<string> NameParts = nameParts.ToList();
            List<string> DescParts = descParts.ToList();

            RemoveBankingTerms(NameParts);
            RemoveBankingTerms(DescParts);

            if (NameParts.Count == 0 || DescParts.Count == 0)
            {
                return false;
            }

            int noOfMatches = 0;
            foreach (string namepart in NameParts)
            {
                string NamePart = namepart.Trim();
                if (NamePart.Length <= 2)
                {
                    continue;
                }
                foreach (string descPart in DescParts)
                {
                    string DescPart = descPart.Trim();
                    if (DescPart.Length <= 2)
                    {
                        continue;
                    }
                    if (DescPart.Contains(NamePart))
                    {
                        noOfMatches++;
                        break;
                    }
                }
            }

            if (noOfMatches == NameParts.Count || noOfMatches == DescParts.Count)
            {
                return true;
            }

            return false;
        }
        public static bool ApproximateMatchCBDescInBKSDesc(string cbDescription, string bksDescription)
        {
            cbDescription = cbDescription.ToUpper();
            bksDescription = bksDescription.ToUpper();

            string[] nameParts = cbDescription.Split(new char[] { ' ', '.', '@', '-', ':', '/', '#', '&', '\\', ',' });
            string[] descParts = bksDescription.Split(new char[] { ' ', '.', '@', '-', ':', '/', '#', '&', '\\', ',' });

            List<string> NameParts = nameParts.ToList();
            List<string> DescParts = descParts.ToList();

            RemoveBankingTerms(NameParts);
            RemoveBankingTerms(DescParts);

            if (NameParts.Count == 0 || DescParts.Count == 0)
            {
                return false;
            }

            int noOfMatches = 0;

            foreach (string namepart in NameParts)
            {
                string NamePart = namepart.Trim();
                if (NamePart.Length <= 3)
                {
                    continue;
                }
                foreach (string descPart in DescParts)
                {
                    string DescPart = descPart.Trim();

                    if (DescPart.Length <= 3)
                    {
                        continue;
                    }

                    int rank = Difference(DescPart, NamePart);
                    if (rank > 4)
                    {
                        noOfMatches++;
                        break;
                    }
                }
            }

            int namePercentage = (int)(((float)noOfMatches / (float)NameParts.Count) * 100);
            int descPercentage = (int)(((float)noOfMatches / (float)DescParts.Count) * 100);

            if (namePercentage >= 75 || descPercentage >= 75)
            {
                return true;
            }

            return false;
        }


        public static bool CheckReferenceWithDescription(string reference, string description)
        {
            reference = reference.Trim().ToUpper();
            description = description.Trim().ToUpper();

            if (string.IsNullOrEmpty(reference) || string.IsNullOrEmpty(description))
            {
                return false;
            }

            string[] refParts = reference.Split(refColShortSplitChars);
            string[] descParts = description.Split(descColSplitChars);

            foreach (string refPartCopy in refParts)
            {
                string refPart = refPartCopy.Trim();
                refPart = refPart.TrimStart('0');
                if (string.IsNullOrEmpty(refPart))
                {
                    continue;
                }
                foreach (string descPartCopy in descParts)
                {
                    string descPart = descPartCopy.Trim();

                    descPart = descPart.TrimStart('0');
                    if (string.IsNullOrEmpty(descPart))
                    {
                        continue;
                    }

                    if (refPart == descPart)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        public static bool CheckReferenceWithReference(string reference1, string reference2)
        {
            reference1 = reference1.Trim().ToUpper();
            reference2 = reference2.Trim().ToUpper();

            if (string.IsNullOrEmpty(reference1) || string.IsNullOrEmpty(reference2))
            {
                return false;
            }

            string[] ref1Parts = reference1.Split(refColShortSplitChars);
            string[] ref2Parts = reference2.Split(refColShortSplitChars);

            if (ref2Parts.Length < ref1Parts.Length)
            {
                string[] temp = ref2Parts;
                ref2Parts = ref1Parts;
                ref1Parts = temp;
            }
            foreach (string ref1PartCopy in ref1Parts)
            {
                string ref1Part = ref1PartCopy.Trim();
                ref1Part = ref1Part.TrimStart('0');
                if (string.IsNullOrEmpty(ref1Part))
                {
                    continue;
                }

                foreach (string ref2PartCopy in ref2Parts)
                {
                    string ref2Part = ref2PartCopy.Trim();
                    ref2Part = ref2Part.TrimStart('0');
                    if (string.IsNullOrEmpty(ref2Part))
                    {
                        continue;
                    }

                    if (ref1Part == ref2Part)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static bool CheckDescriptionWithDescription(string description1, string description2)
        {
            description1 = description1.Trim().ToUpper();
            description2 = description2.Trim().ToUpper();

            if (string.IsNullOrEmpty(description1) || string.IsNullOrEmpty(description2))
            {
                return false;
            }

            string[] desc1Parts = description1.Split(descColSplitChars);
            string[] desc2Parts = description2.Split(descColSplitChars);

            if (desc1Parts.Length <= 0 || desc2Parts.Length <= 0)
            {
                return false;
            }

            if (desc2Parts.Length < desc1Parts.Length)
            {
                string[] temp = desc2Parts;

                desc2Parts = desc1Parts;
                desc1Parts = temp;
            }

            int noOfMatches = 0;
            int largeWordMatchCount = 0;

            foreach (string desc1PartCopy in desc1Parts)
            {
                string desc1Part = desc1PartCopy.Trim();

                if (string.IsNullOrEmpty(desc1Part))
                {
                    continue;
                }

                foreach (string desc2PartCopy in desc2Parts)
                {
                    string desc2Part = desc2PartCopy.Trim();

                    if (string.IsNullOrEmpty(desc2Part))
                    {
                        continue;
                    }

                    if (desc1Part == desc2Part)
                    {
                        noOfMatches++;
                        if (desc1Part.Length >= 5 && desc2Part.Length >= 5)
                        {
                            largeWordMatchCount++;
                        }
                    }
                    else
                    {
                        if (desc1Part.Length >= 5 && desc2Part.Length >= 5)
                        {
                            int rank = PatternMatchingManager.Difference(desc1Part, desc2Part);

                            if (rank > 3)
                            {
                                noOfMatches++;
                                largeWordMatchCount++;
                            }
                        }
                    }
                }
            }

            if (desc1Parts.Length <= 2)
            {
                if (noOfMatches == desc1Parts.Length)
                {
                    return true;
                }
            }
            else
            {
                if (noOfMatches > 2)
                {
                    return true;
                }
            }

            if (largeWordMatchCount > 1)
            {
                return true;
            }

            return false;
        }
        public static bool PercentageCheckDescriptionWithDescription(string description1, string description2)
        {
            description1 = description1.Trim().ToUpper();
            description2 = description2.Trim().ToUpper();

            string[] desc1Parts = description1.Split(descColSplitChars);
            string[] desc2Parts = description2.Split(descColSplitChars);
            if (desc1Parts.Length == 0 || desc2Parts.Length == 0)
            {
                return false;
            }

            int noOfMatches = 0;

            foreach (string desc1PartCopy in desc1Parts)
            {
                string desc1Part = desc1PartCopy.Trim();
                if (desc1Part.Length <= 3)
                {
                    continue;
                }
                foreach (string desc2PartCopy in desc2Parts)
                {
                    string desc2Part = desc2PartCopy.Trim();

                    if (desc2Part.Length <= 3)
                    {
                        continue;
                    }

                    int rank = Difference(desc1Part, desc2Part);
                    if (rank > 4)
                    {
                        noOfMatches++;
                        break;
                    }
                }
            }

            int percentage = (int)(((float)noOfMatches / (float)desc1Parts.Length) * 100);

            if (percentage >= 75)
            {
                return true;
            }

            noOfMatches = 0;
            foreach (string desc2PartCopy in desc2Parts)
            {
                string desc2Part = desc2PartCopy.Trim();
                if (desc2Part.Length <= 3)
                {
                    continue;
                }
                foreach (string desc1PartCopy in desc1Parts)
                {
                    string desc1Part = desc1PartCopy.Trim();

                    if (desc1Part.Length <= 3)
                    {
                        continue;
                    }

                    int rank = Difference(desc1Part, desc2Part);
                    if (rank > 4)
                    {
                        noOfMatches++;
                        break;
                    }
                }
            }

            percentage = (int)(((float)noOfMatches / (float)desc2Parts.Length) * 100);

            if (percentage >= 75)
            {
                return true;
            }

            return false;
        }


        public static bool CheckCBTellerInDescription(string cbReference, string bksReference)
        {
            cbReference = cbReference.Trim().ToUpper();
            bksReference = bksReference.Trim().ToUpper();

            string[] tellers = cbReference.Split(new char[] { '-', ':', ' ', '/', '\\' });
            foreach (string teller in tellers)
            {
                string Teller = teller.Trim();

                if (!IsNumeric(Teller))
                {
                    continue;
                }

                Teller = teller.TrimStart('0');
                if (string.IsNullOrEmpty(Teller))
                {
                    continue;
                }
                if (bksReference.Contains(Teller))
                {
                    return true;
                }
            }

            return false;
        }
        public static bool CheckCBTellerInReference(string cbReference, string bksReference)
        {
            cbReference = cbReference.Trim().ToUpper();
            bksReference = bksReference.Trim().ToUpper();

            string[] tellers = cbReference.Split(new char[] { '-', ':', ' ', '/', '\\' });
            foreach (string teller in tellers)
            {
                string Teller = teller.Trim();

                if (!IsNumeric(Teller))
                {
                    continue;
                }

                Teller = teller.TrimStart('0');
                if (string.IsNullOrEmpty(Teller))
                {
                    continue;
                }
                if (bksReference.Equals(Teller))
                {
                    return true;
                }
            }

            return false;
        }
        public static bool CheckReversedRecord(string description)
        {
            string reversed = "REVERSED";
            description = description.ToUpper();

            if (description.Contains(reversed))
            {
                return true;
            }
            return false;
        }

        public static string GetCustomerNameFromCB(string cbDescription)
        {
            //Get the customer name from the Description field of CashBook
            int index = cbDescription.IndexOf('-');
            if (index < 0)
            {
                return string.Empty;
            }
            string zunk = cbDescription.Substring(index + 1);
            index = zunk.IndexOf('-');
            if (index < 0)
            {
                return zunk;
            }
            return zunk.Substring(0, index);
        }

        public static void RemoveBankingTerms(List<string> parts)
        {
            bool atLeastOneMatch = true;


            while (atLeastOneMatch)
            {
                atLeastOneMatch = false;

                if (parts.Contains(string.Empty))
                {
                    parts.Remove(string.Empty);
                    atLeastOneMatch = true;
                }
                if (parts.Contains("PYT"))
                {
                    parts.Remove("PYT");
                    atLeastOneMatch = true;
                }
                if (parts.Contains("CASH"))
                {
                    parts.Remove("CASH");
                    atLeastOneMatch = true;
                }
                if (parts.Contains("CSH"))
                {
                    parts.Remove("CSH");
                    atLeastOneMatch = true;
                }
                if (parts.Contains("BY"))
                {
                    parts.Remove("BY");
                    atLeastOneMatch = true;
                }
                if (parts.Contains("FOR"))
                {
                    parts.Remove("FOR");
                    atLeastOneMatch = true;
                }
                if (parts.Contains("DEP"))
                {
                    parts.Remove("DEP");
                    atLeastOneMatch = true;
                }
                if (parts.Contains("CDB"))
                {
                    parts.Remove("CDB");
                    atLeastOneMatch = true;
                }
                if (parts.Contains("ON"))
                {
                    parts.Remove("ON");
                    atLeastOneMatch = true;
                }
                if (parts.Contains("ACCT"))
                {
                    parts.Remove("ACCT");
                    atLeastOneMatch = true;
                }
                if (parts.Contains("4"))
                {
                    parts.Remove("4");
                    atLeastOneMatch = true;
                }
                if (parts.Contains("ACCOUNT"))
                {
                    parts.Remove("ACCOUNT");
                    atLeastOneMatch = true;
                }
                if (parts.Contains("AGO"))
                {
                    parts.Remove("AGO");
                    atLeastOneMatch = true;
                }
                if (parts.Contains("DPK"))
                {
                    parts.Remove("DPK");
                    atLeastOneMatch = true;
                }
                if (parts.Contains("PMS"))
                {
                    parts.Remove("PMS");
                    atLeastOneMatch = true;
                }
                if (parts.Contains("INV"))
                {
                    parts.Remove("INV");
                    atLeastOneMatch = true;
                }
            }

            RemoveNumbers(parts);
        }
        public static void RemoveNumbers(List<string> values)
        {
            List<string> toDelete = new List<string>();
            int dummy = 0;
            foreach (string value in values)
            {
                if (int.TryParse(value, out dummy))
                {
                    toDelete.Add(value);
                }
            }

            foreach (string s in toDelete)
            {
                values.Remove(s);
            }
        }

        public static List<string> GetReferenceNosFromDescription(string description)
        {
            List<string> referenceNos = new List<string>();
            string[] parts = description.Split(new char[] { ' ', '.', '@', '-', ':', '/', '#', '&', '\\', ',' });

            long result = 0;
            foreach (string part in parts)
            {
                if (Int64.TryParse(part, out result))
                {
                    if (part.Length > 3)
                    {
                        referenceNos.Add(part);
                    }
                }
            }

            return referenceNos;
        }
        public static bool IsReversal(string description, string reference)
        {
            try
            {
                if (string.IsNullOrEmpty(reference))
                {
                    return false;
                }

                string[] descriptions = description.Split(splitChars);

                if (descriptions == null || descriptions.Length <= 0)
                {
                    return false;
                }

                List<string> descriptionParts = descriptions.ToList();

                foreach (string descriptionPart in descriptionParts)
                {
                    if (descriptionPart == reference)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }
        public static bool IsBKSReversal(string description, string description1)
        {
            List<string> firstReferences = GetReferenceNosFromDescription(description);

            if (firstReferences.Count <= 0)
            {
                return false;
            }

            List<string> secondReferences = GetReferenceNosFromDescription(description1);

            if (secondReferences.Count <= 0)
            {
                return false;
            }

            if (firstReferences.Count > secondReferences.Count)
            {
                foreach (string secondReference in secondReferences)
                {
                    bool isFound = false;
                    foreach (string firstReference in firstReferences)
                    {
                        if (firstReference == secondReference)
                        {
                            isFound = true;
                        }
                    }
                    if (!isFound)
                    {
                        return false;
                    }
                }
            }
            else
            {
                foreach (string firstReference in firstReferences)
                {
                    bool isFound = false;
                    foreach (string secondReference in secondReferences)
                    {
                        if (firstReference == secondReference)
                        {
                            isFound = true;
                        }
                    }
                    if (!isFound)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        public static bool IsCBReversal(string description, string description1)
        {
            string[] firstDescParts = description.Split(new char[] { ' ', '.', '@', '-', ':', '/', '#', '&', '\\', ',' });
            string[] secondDescParts = description1.Split(new char[] { ' ', '.', '@', '-', ':', '/', '#', '&', '\\', ',' });

            if (firstDescParts.Length <= 0 || secondDescParts.Length <= 0)
            {
                return false;
            }
            List<string> firstReferences = firstDescParts.ToList();
            List<string> secondReferences = secondDescParts.ToList();

            if (firstReferences.Count > secondReferences.Count)
            {
                foreach (string secondReference in secondReferences)
                {
                    bool isFound = false;
                    foreach (string firstReference in firstReferences)
                    {
                        if (firstReference == secondReference)
                        {
                            isFound = true;
                        }
                    }
                    if (!isFound)
                    {
                        return false;
                    }
                }
            }
            else
            {
                foreach (string firstReference in firstReferences)
                {
                    bool isFound = false;
                    foreach (string secondReference in secondReferences)
                    {
                        if (firstReference == secondReference)
                        {
                            isFound = true;
                        }
                    }
                    if (!isFound)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        #region [ Key word search ]
        public static bool PerfectMatchKeyword(string keyword, string description)
        {
            keyword = keyword.ToUpper();
            description = description.ToUpper();

            string[] keywordParts = keyword.Split(' ');

            string[] descParts = description.Split(new char[] { ' ', '.', '@', '-', ':', '/', '#', '&', '/' });

            int noOfMatches = 0;
            foreach (string keywordPart in keywordParts)
            {
                foreach (string descPart in descParts)
                {
                    string copyDescPart = descPart.Trim();
                    if (copyDescPart.Equals(keywordPart))
                    {
                        noOfMatches++;
                        break;
                    }
                }
            }

            if (noOfMatches == keywordParts.Length)
            {
                return true;
            }

            return false;
        }
        public static bool ApproximateMatchKeyword(string keyword, string description)
        {
            keyword = keyword.ToUpper();
            description = description.ToUpper();

            string[] keywordParts = keyword.Split(' ');

            string[] descParts = description.Split(new char[] { ' ', '.', '@', '-', ':', '/', '#', '&', '/' });
            int noOfMatches = 0;
            foreach (string keywordPart in keywordParts)
            {
                foreach (string descPart in descParts)
                {
                    string copyDescPart = descPart.Trim();
                    if (copyDescPart == string.Empty)
                    {
                        continue;
                    }
                    if (copyDescPart.Length <= 2)
                    {
                        continue;
                    }
                    int rank = Difference(keywordPart, copyDescPart);
                    if (rank > 4)
                    {
                        noOfMatches++;
                        break;
                    }
                }
            }

            if (noOfMatches == keywordParts.Length)
            {
                return true;
            }

            return false;
        }
        #endregion

        #region [ Helpers ]
        private static bool IsNumeric(string value)
        {
            double result = 0;

            return Double.TryParse(value, out result);
        }
        #endregion
    }
}