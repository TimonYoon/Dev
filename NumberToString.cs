using UnityEngine;
using System.Collections;
using System;
using System.Linq;


public static class NumberToStringExtensionMethods
{
    static string[] suffixs = new string[] { string.Empty, "K", "M", "B", "T", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

    /// <summary> 514.3U 이런 형식의 텍스트로 바꿔줌 </summary>
    public static string ToStringABC(this double number)
    {
        int mag = (int)(Math.Floor(Math.Log10(number)) / 3); // Truncates to 6, divides to 2
        double divisor = Math.Pow(10, mag * 3);

        double shortNumber = number / divisor;



        string suffix = string.Empty;

        string result = string.Empty;


        if (number == 0d)
        {
            result = "0";
        }
        else if (mag < 1)
        {
            result = shortNumber.ToString("N0");// ((int)shortNumber).ToString();
        }

        else if (mag < suffixs.Length)
        {
            suffix = suffixs[mag];
            result = shortNumber.ToString("N1") + suffix;
        }
        else
        {
            result = shortNumber.ToString("E2");
        }

        return result;
    }

    /// <summary> 1,234,567 이런 형식의 콤마를 붙인 텍스트로 바꿔줌 </summary>
    public static string ToStringComma(this double number)
    {

        string returnData = string.Empty;

        string data = number.ToString();

        char[] charArray = data.ToCharArray().Reverse().ToArray();

        for (int i = 0; i < charArray.Length; i++)
        {
            returnData = charArray[i] + returnData;
            if ((i + 1) % 3 == 0 && (i + 1) < charArray.Length)
                returnData = ',' + returnData;
        }
        return returnData;
    }

    /// <summary> 1h 23m 또는 12m 45s 이런 형식의 시간 텍스트로 바꿔줌 </summary>
    public static string ToStringTimeHMS(this float number)
    {
        //return "13m 25s";
        string result = string.Empty;
        
        if (number > 3600)
        {
            float a = number % 3600;
            float hour = (number - a) / 3600;

            float b = a % 60;

            float minute = (a - b) / 60;

            result = hour + "h" + minute + "m";
        }
        else if (number > 60)
        {
            float a = number % 60;

            float minute = (number - a) / 60;

            result = minute + "m" + a.ToString("N0") + "s";
        }
        else
        {
            result = number.ToString("N0") + "s";
        }

        return result;
    }

    /// <summary> 1:23:45 이런 형식의 시간 텍스트로 바꿔줌 </summary>
    public static string ToStringTime(this float number)
    {
        string result = string.Empty;
        
        if (number > 3600)
        {
            float a = number % 3600;
            float hour = (number - a) / 3600;

            float b = a % 60;
            float minute = (a - b) / 60;


            float second = b;

            string h = hour / 10 < 1 ? "0" + hour : hour.ToString();
            string m = minute / 10 < 1 ? "0" + minute : minute.ToString();
            string s = second / 10 < 1 ? "0" + second.ToString().Split('.')[0] : second.ToString().Split('.')[0];

            result = h + ":" + m + ":" + s;
        }
        else if (number > 60)
        {
            float a = number % 60;

            float minute = (number - a) / 60;

            string m = minute / 10 < 1 ? "0" + minute : minute.ToString();
            string s = a / 10 < 1 ? "0" + a.ToString().Split('.')[0] : a.ToString().Split('.')[0];

            result = m + ":" + s;
        }
        else
        {
            string s = number / 10 < 1 ? "0" + number.ToString().Split('.')[0] : number.ToString().Split('.')[0];
            result = "00:" + s;
        }

        return result;
    }
}