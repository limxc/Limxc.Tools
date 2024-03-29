﻿// Copyright (c) 2013-2018 Cemalettin Dervis, MIT License.
// https://github.com/cemdervis/SharpConfig

// ReSharper disable InconsistentNaming


// Copyright (c) 2013-2018 Cemalettin Dervis, MIT License.
// https://github.com/cemdervis/SharpConfig

// ReSharper disable InconsistentNaming

namespace Limxc.Tools.Core.Dependencies.SharpConfig
{
    // Enumerates the elements of a Setting that represents an array.
    internal sealed class SettingArrayEnumerator
    {
        private readonly int mLastRBraceIdx;
        private readonly bool mShouldCalcElemString;
        private readonly string mStringValue;
        private int mBraceBalance;
        private int mIdxInString;
        private bool mIsDone;
        private bool mIsInQuotes;
        private int mPrevElemIdxInString;

        public SettingArrayEnumerator(string value, bool shouldCalcElemString)
        {
            mStringValue = value;
            mIdxInString = -1;
            mLastRBraceIdx = -1;
            mShouldCalcElemString = shouldCalcElemString;
            IsValid = true;
            mIsDone = false;

            for (var i = 0; i < value.Length; ++i)
            {
                var ch = value[i];
                if (ch != ' ' && ch != '{')
                    break;

                if (ch == '{')
                {
                    mIdxInString = i + 1;
                    mBraceBalance = 1;
                    mPrevElemIdxInString = i + 1;
                    break;
                }
            }

            // Abort if no valid '{' occurred.
            if (mIdxInString < 0)
            {
                IsValid = false;
                mIsDone = true;
                return;
            }

            // See where the last valid '}' is.
            for (var i = value.Length - 1; i >= 0; --i)
            {
                var ch = value[i];
                if (ch != ' ' && ch != '}')
                    break;

                if (ch == '}')
                {
                    mLastRBraceIdx = i;
                    break;
                }
            }

            // Abort if no valid '}' occurred.
            if (mLastRBraceIdx < 0)
            {
                IsValid = false;
                mIsDone = true;
                return;
            }

            // See if this is an empty array such as "{    }" or "{}".
            // If so, this is a valid array, but with size 0.
            if (
                mIdxInString == mLastRBraceIdx
                || !IsNonEmptyValue(mStringValue, mIdxInString, mLastRBraceIdx)
            )
            {
                IsValid = true;
                mIsDone = true;
            }
        }

        public string Current { get; private set; }

        public bool IsValid { get; private set; }

        private void UpdateElementString(int idx)
        {
            Current = mStringValue.Substring(mPrevElemIdxInString, idx - mPrevElemIdxInString);

            Current = Current.Trim(' '); // trim spaces first

            // Now trim the quotes, but only the first and last, because
            // the setting value itself can contain quotes.
            if (Current[Current.Length - 1] == '\"')
                Current = Current.Remove(Current.Length - 1, 1);

            if (Current[0] == '\"')
                Current = Current.Remove(0, 1);
        }

        public bool Next()
        {
            if (mIsDone)
                return false;

            var idx = mIdxInString;
            while (idx <= mLastRBraceIdx)
            {
                var ch = mStringValue[idx];
                if (ch == '{' && !mIsInQuotes)
                {
                    ++mBraceBalance;
                }
                else if (ch == '}' && !mIsInQuotes)
                {
                    --mBraceBalance;
                    if (idx == mLastRBraceIdx)
                    {
                        // This is the last element.
                        if (!IsNonEmptyValue(mStringValue, mPrevElemIdxInString, idx))
                            // Empty array element; invalid array.
                            IsValid = false;
                        else if (mShouldCalcElemString)
                            UpdateElementString(idx);
                        mIsDone = true;
                        break;
                    }
                }
                else if (ch == '\"')
                {
                    var iNextQuoteMark = mStringValue.IndexOf('\"', idx + 1);
                    if (iNextQuoteMark > 0 && mStringValue[iNextQuoteMark - 1] != '\\')
                    {
                        idx = iNextQuoteMark;
                        mIsInQuotes = false;
                    }
                    else
                    {
                        mIsInQuotes = true;
                    }
                }
                else if (
                    ch == Configuration.ArrayElementSeparator
                    && mBraceBalance == 1
                    && !mIsInQuotes
                )
                {
                    if (!IsNonEmptyValue(mStringValue, mPrevElemIdxInString, idx))
                        // Empty value in-between commas; this is an invalid array.
                        IsValid = false;
                    else if (mShouldCalcElemString)
                        UpdateElementString(idx);

                    mPrevElemIdxInString = idx + 1;

                    // Yield.
                    ++idx;
                    break;
                }

                ++idx;
            }

            mIdxInString = idx;

            if (mIsInQuotes)
                IsValid = false;

            return IsValid;
        }

        private static bool IsNonEmptyValue(string s, int begin, int end)
        {
            for (; begin < end; ++begin)
                if (s[begin] != ' ')
                    return true;

            return false;
        }
    }
}