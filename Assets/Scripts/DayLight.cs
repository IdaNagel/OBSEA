using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class DayLight
{
    private static double DegToRad => Math.PI / 180.0;
    private static double RadToDeg => 180.0 / Math.PI;

    /// <summary>
    /// Calcula la rotación de una Directional Light en Unity para simular la posición del Sol.
    /// </summary>
    /// <param name="dateTime">DateTime</param>
    /// <param name="latitude">Latitude degrees (+N -S)</param>
    /// <param name="longitude">Longitude degrees (+E -W)</param>
    /// <returns>Vector3 con los ángulos de Euler (x, y, z) para la Directional Light.</returns>
    public static Vector3 GetLightRotation(DateTime dateTime, double latitude, double longitude)
    {
        double degToRad = DegToRad;
        double radToDeg = RadToDeg;

        double jd = DateTimeToJulianDay(dateTime);

        var (_, _, _, _, _, delta, E) = CalculateSolarParameters(jd);

        // Calcular ángulo horario
        double hourAngle = CalculateHourAngle(dateTime, longitude, E);

        // Convertir a coordenadas locales (altitud y azimut)
        double sinAlt = Math.Sin(degToRad * latitude) * Math.Sin(degToRad * delta) +
                        Math.Cos(degToRad * latitude) * Math.Cos(degToRad * delta) * Math.Cos(degToRad * hourAngle);
        double altitude = Math.Asin(sinAlt) * radToDeg;

        double cosAz = (Math.Sin(degToRad * delta) - Math.Sin(degToRad * latitude) * sinAlt) /
                       (Math.Cos(degToRad * latitude) * Math.Cos(Math.Asin(sinAlt)));
        double azimuth = Math.Acos(cosAz) * radToDeg;
        if (Math.Sin(degToRad * hourAngle) >= 0)
            azimuth = 360 - azimuth; // Ajustar azimut para H > 0 (después del mediodía)

        // Mapear altitud y azimut a ángulos de Euler para Unity
        float x = (float)(180.0 - altitude); // Altitud 0° → X=180, 90° → X=90, 180° → X=0
        float y = (float)azimuth;
        float z = 0.0f; // Sin rotación en Z

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Get Sunrise and Sunset time for a date/location
    /// </summary>
    /// <param name="date">Date</param>
    /// <param name="latitude">Latitude degrees (+N -S)</param>
    /// <param name="longitude">Longitude degrees (+E -W)</param>
    /// <returns>UTC DateTimes for sunrise and sunset</returns>
    public static (DateTime sunrise, DateTime sunset) GetSunTime(DateTime date, double latitude, double longitude)
    {
        double degToRad = DegToRad;
        double radToDeg = RadToDeg;

        DateTime dateMidnight = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
        double jd = DateTimeToJulianDay(dateMidnight);

        var (_, _, _, _, _, delta, E) = CalculateSolarParameters(jd);

        // Ángulo horario para salida/puesta del sol (H0)
        double cosH0 = (Math.Cos(degToRad * 90.833) - Math.Sin(degToRad * latitude) * Math.Sin(degToRad * delta)) /
                       (Math.Cos(degToRad * latitude) * Math.Cos(degToRad * delta));
        if (cosH0 < -1 || cosH0 > 1)
            throw new ArgumentException("There is no Sunrise or Sunset for this date/location.");

        double H0 = Math.Acos(cosH0) * radToDeg; // Ángulo horario en grados

        // Tiempo solar medio para salida y puesta
        double meanSolarTimeSunrise = (720 - 4 * (longitude + H0) - E) / 1440.0; // Fracción de día
        double meanSolarTimeSunset = (720 - 4 * (longitude - H0) - E) / 1440.0;  // Fracción de día

        // To Julian Day
        double jdSunrise = jd + meanSolarTimeSunrise;
        double jdSunset = jd + meanSolarTimeSunset;

        // To UTC DateTime
        DateTime sunrise = JulianDayToDateTime(jdSunrise);
        DateTime sunset = JulianDayToDateTime(jdSunset);

        return (sunrise, sunset);
    }
    private static void TestGetSunTime(double latitude, double longitude)
    {
        StringBuilder logBuilder = new StringBuilder();
        DateTime startDate = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime endDate = new DateTime(2040, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
        {
            try
            {
                var (sunrise, sunset) = GetSunTime(date, latitude, longitude);
                string line = $"Date: {date:yyyyMMdd} Sunrise: {sunrise:yyyyMMddHHmmss} Sunset: {sunset:yyyyMMddHHmmss}\n";
                logBuilder.Append(line);
            }
            catch (ArgumentException ex)
            {
                string line = $"Date: {date:yyyyMMdd} Error: {ex.Message}\n";
                logBuilder.Append(line);
            }
        }

        using (StreamWriter file = new StreamWriter(Path.Combine(Application.persistentDataPath, "sun_times.txt")))
        {
            file.WriteLine(logBuilder.ToString());
        }
    }

    /// <summary>
    /// Convert UTC Date to Julian Day
    /// </summary>
    /// <param name="date">Datetime (UTC)</param>
    /// <returns>Julian Day</returns>
    private static double DateTimeToJulianDay(DateTime date)
    {
        int year = date.Year;
        int month = date.Month;
        int day = date.Day;
        double hours = date.Hour + date.Minute / 60.0 + date.Second / 3600.0;

        if (month <= 2)
        {
            year--;
            month += 12;
        }

        int A = year / 100;
        int B = 2 - A + (A / 4);        // Gregorian fix

        double JD = Math.Floor(365.25 * (year + 4716)) +
                    Math.Floor(30.6001 * (month + 1)) +
                    day + B - 1524.5 + hours / 24.0;

        return JD;
    }
    private static void TestDateTimeToJulianDay()
    {
        StringBuilder logBuilder = new StringBuilder();
        DateTime startDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime endDate = new DateTime(2100, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        double baseJD = 2415020.5; // JD para 1 de enero de 1900, 00:00 UTC
        const double tolerance = 0.0001;
        double previousDifference = -10000;

        for (DateTime date = startDate; date <= endDate; date = date.AddHours(1))
        {
            double result = DateTimeToJulianDay(date);
            double expectedJD = baseJD + (date - startDate).TotalDays;
            double difference = Math.Abs(result - expectedJD);
            bool passed = difference < tolerance;

            if (Math.Abs(previousDifference - difference) > 0.000001)
            {
                string line = $"Test: {date:yyyyMMddHHmmss} Res: {result:F6} Exp: {expectedJD:F6} Diff: {difference:F6} Stat: {(passed ? "OK" : "NOT OK")}\n";
                logBuilder.Append(line);
            }

            previousDifference = difference;
        }

        using (StreamWriter file = new StreamWriter(Path.Combine(Application.persistentDataPath, "results_datetime_to_jd.txt")))
        {
            file.WriteLine(logBuilder.ToString());
        }
    }

    /// <summary>
    /// Convert Julian Day to UTC Date
    /// </summary>
    /// <param name="jd">Julian Day</param>
    /// <returns>DateTime (UTC)</returns>
    private static DateTime JulianDayToDateTime(double jd)
    {
        if (double.IsNaN(jd) || double.IsInfinity(jd))
            throw new ArgumentException("Julian Day invalid: " + jd);

        jd += 0.5; // Ajuste para que el día comience a medianoche en lugar de mediodía
        double Z = Math.Floor(jd); // Parte entera
        double F = jd - Z; // Parte fraccionaria (para horas, minutos, segundos)

        int alpha = (int)Math.Floor((Z - 1867216.25) / 36524.25);
        double A = Z + 1 + alpha - (int)Math.Floor(alpha / 4.0);        // Gregorian fix

        double B = A + 1524;
        int C = (int)Math.Floor((B - 122.1) / 365.25);
        int D = (int)Math.Floor(365.25 * C);
        int E = (int)Math.Floor((B - D) / 30.6001);

        int day = (int)(B - D - Math.Floor(30.6001 * E));
        int month = E < 14 ? E - 1 : E - 13;
        int year = month > 2 ? C - 4716 : C - 4715;

        double hoursFraction = F * 24.0;
        int hours = (int)Math.Floor(hoursFraction);
        double minutesFraction = (hoursFraction - hours) * 60.0;
        int minutes = (int)Math.Floor(minutesFraction);
        double secondsFraction = (minutesFraction - minutes) * 60.0;
        int seconds = (int)Math.Floor(secondsFraction);

        // Ajustar posibles desbordamientos debido a redondeos
        if (seconds >= 60)
        {
            seconds -= 60;
            minutes++;
        }
        if (minutes >= 60)
        {
            minutes -= 60;
            hours++;
        }
        if (hours >= 24)
        {
            hours -= 24;
            day++;
        }

        // Crear el objeto DateTime
        try
        {
            return new DateTime(year, month, day, hours, minutes, seconds, DateTimeKind.Utc);
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new ArgumentException($"El Día Juliano {jd - 0.5} no puede convertirse en una fecha válida en el rango de DateTime.");
        }
    }
    private static void TestJulianDayToDateTime()
    {
        StringBuilder logBuilder = new StringBuilder();
        double startJD = 2415020.5; // JD para 1 de enero de 1900, 00:00 UTC
        double endJD = 2488069.5;   // JD aproximado para 31 de diciembre de 2100, 00:00 UTC
        DateTime baseDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        const double tolerance = 0.0001;
        double previousDifference = -10000;

        for (double jd = startJD; jd <= endJD; jd += 1.0) // next JD
        {
            try
            {
                DateTime result = JulianDayToDateTime(jd);
                DateTime expectedDate = baseDate.AddDays(jd - startJD);

                double resultJD = DateTimeToJulianDay(result);
                double difference = Math.Abs(resultJD - jd);
                bool passed = difference < tolerance;

                if (Math.Abs(previousDifference - difference) > 0.000001)
                {
                    string line = $"Test: {jd:F6} Res: {result:yyyyMMddHHmmss} Exp: {expectedDate:yyyyMMddHHmmss} Diff: {difference:F6} Stat: {(passed ? "OK" : "NOT OK")}\n";
                    logBuilder.Append(line);
                }

                previousDifference = difference;
            }
            catch (ArgumentException ex)
            {
                logBuilder.Append($"Test: {jd:F6} Error: {ex.Message}\n");
            }
        }

        using (StreamWriter file = new StreamWriter(Path.Combine(Application.persistentDataPath, "results_jd_to_datetime.txt")))
        {
            file.WriteLine(logBuilder.ToString());
        }
    }

    /// <summary>
    /// Calculate astronomic parameters for a Julian Day
    /// </summary>
    /// <param name="jd">Julian Day</param>
    /// <returns>L (longitud eclíptica media), M (anomalía media), C (ecuación del centro),
    /// lambda (longitud verdadera), epsilon (oblicuidad), delta (declinación), E (ecuación del tiempo en minutos).</returns>
    private static (double L, double M, double C, double lambda, double epsilon, double delta, double E) CalculateSolarParameters(double jd)
    {
        double degToRad = Math.PI / 180.0;
        double radToDeg = 180.0 / Math.PI;

        // Calcular el siglo juliano (T) desde J2000.0
        double T = (jd - 2451545.0) / 36525.0;

        // Longitud eclíptica media del Sol (L)
        double L = (280.46646 + 36000.76983 * T + 0.0003032 * T * T) % 360;
        if (L < 0) L += 360;

        // Anomalía media del Sol (M)
        double M = (357.52911 + 35999.05029 * T - 0.0001537 * T * T) % 360;
        if (M < 0) M += 360;

        // Ecuación del centro (C)
        double C = (1.914602 - 0.004817 * T - 0.000014 * T * T) * Math.Sin(degToRad * M) +
                   (0.019993 - 0.000101 * T) * Math.Sin(degToRad * 2 * M) +
                   0.000289 * Math.Sin(degToRad * 3 * M);

        // Longitud eclíptica verdadera del Sol (lambda)
        double lambda = L + C;

        // Oblicuidad de la eclíptica (epsilon)
        double epsilon = 23.439291 - 0.0130042 * T - 0.000000163 * T * T;

        // Declinación del Sol (delta)
        double delta = Math.Asin(Math.Sin(degToRad * lambda) * Math.Sin(degToRad * epsilon)) * radToDeg;

        // Ecuación del tiempo (E, en minutos)
        double y = Math.Tan(degToRad * epsilon / 2.0);
        y *= y;
        double E = y * Math.Sin(2 * degToRad * L) -
                   2 * 0.016708 * Math.Sin(degToRad * M) +
                   4 * 0.016708 * y * Math.Sin(degToRad * M) * Math.Cos(2 * degToRad * L) -
                   0.5 * y * y * Math.Sin(4 * degToRad * L) -
                   1.25 * 0.016708 * 0.016708 * Math.Sin(2 * degToRad * M);
        E *= 4 * radToDeg; // Convertir a minutos

        return (L, M, C, lambda, epsilon, delta, E);
    }

    /// <summary>
    /// Calculate Sun angle for DateTime and Longitude
    /// </summary>
    /// <param name="date">UTC DateTime</param>
    /// <param name="longitude">Longitude degrees (+E -W)</param>
    /// <param name="E">Ecuación del tiempo en minutos</param>
    /// <returns>Solar hour angle in degrees</returns>
    private static double CalculateHourAngle(DateTime date, double longitude, double E)
    {
        double hours = date.Hour + date.Minute / 60.0 + date.Second / 3600.0;
        double solarTime = hours + E / 60.0 + longitude / 15.0; // Ajuste por longitud (1° = 4 minutos)
        return solarTime / 24.0 * 360.0 - 180.0; // Ángulo horario en grados
    }

    public static void Tests()
    {
        TestDateTimeToJulianDay();
        TestJulianDayToDateTime();

        double latitude = 41.2239; // Vilanova i la Geltrú (N)
        double longitude = 1.7252; // Vilanova i la Geltrú (E)
        TestGetSunTime(latitude, longitude);
    }
}