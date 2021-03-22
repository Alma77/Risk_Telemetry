using System;
using System.Collections.Generic;
using System.Linq;

namespace Risk.Server
{
    public static class ColorGenerator
    {
        static ColorGenerator()
        {
            colors = new Dictionary<string, string>();
        }

        static Queue<string> palette = new Queue<string>(new []{
            "#FFE6E6",
            "#4682B4",
            "#5F9EA0",
            "#6495ED",
            "#6A5ACD",
            "#808080",
            "#8FBC8F",
            "#A9A9A9",
            "#C0C0C0",
            "#FFAAAA",
            "#D46A6A",
            "#801515",
            "#0066FF",
            "#00FFFF",
            "#3399FF",
            "#33FFFF",
            "#9900FF",
            "#9966FF",
            "#9999FF",
            "#99FFFF",
            "#CC33FF",
            "#CCFFFF",
            "#FFCCFF",
            "#5500AA",
            "#FFE6AA",
            "#4682AA",
            "#5F9EAA",
            "#6495AA",
            "#6A5AAA",
            "#8080AA",
            "#8FBCAA",
            "#A9A9AA",
            "#C0C0AA",
            "#FFAAAA",
            "#D46AAA",
            "#8015AA",
            "#0066AA",
            "#00FFAA",
            "#3399AA",
            "#33FFAA",
            "#9900AA",
            "#9966AA",
            "#9999AA",
            "#99FFAA",
            "#CC33AA",
            "#CCFFAA",
            "#FFCCAA",
            "#5500BB",
            "#FFE6BB",
            "#4682BB",
            "#5F9EBB",
            "#6495BB",
            "#6A5ABB",
            "#8080BB",
            "#8FBCBB",
            "#A9A9BB",
            "#C0C0BB",
            "#FFAABB",
            "#D46ABB",
            "#8015BB",
            "#0066BB",
            "#00FFBB",
            "#3399BB",
            "#33FFBB",
            "#9900BB",
            "#9966BB",
            "#9999BB",
            "#99FFBB",
            "#CC33BB",
            "#CCFFBB",
            "#FFCCBB",
            "#5500CC",
            "#FFE6CC",
            "#4682CC",
            "#5F9ECC",
            "#6495CC",
            "#6A5ACC",
            "#8080CC",
            "#8FBCCC",
            "#A9A9CC",
            "#C0C0CC",
            "#FFAACC",
            "#D46ACC",
            "#8015CC",
            "#0066CC",
            "#00FFCC",
            "#3399CC",
            "#33FFCC",
            "#9900CC",
            "#9966CC",
            "#9999CC",
            "#99FFCC",
            "#CC33CC",
            "#CCFFCC",
            "#FFCCCC",
            "#5500DD",
            "#FFE6DD",
            "#4682DD",
            "#5F9EDD",
            "#6495DD",
            "#6A5ADD",
            "#8080DD",
            "#8FBCDD",
            "#A9A9DD",
            "#C0C0DD",
            "#FFAADD",
            "#D46ADD",
            "#8015DD",
            "#0066DD",
            "#00FFDD",
            "#3399DD",
            "#33FFDD",
            "#9900DD",
            "#9966DD",
            "#9999DD",
            "#99FFDD",
            "#CC33DD",
            "#CCFFDD",
            "#FFCCDD",
            "#5500EE",
            "#FFE6EE",
            "#4682EE",
            "#5F9EEE",
            "#6495EE",
            "#6A5AEE",
            "#8080EE",
            "#8FBCEE",
            "#A9A9EE",
            "#C0C0EE",
            "#FFAAEE",
            "#D46AEE",
            "#8015EE",
            "#0066EE",
            "#00FFEE",
            "#3399EE",
            "#33FFEE",
            "#9900EE",
            "#9966EE",
            "#9999EE",
            "#99FFEE",
            "#CC33EE",
            "#CCFFEE",
            "#FFCCEE",
            "#5500EE"
        });

        private static Dictionary<string, string> colors;

        public static string GetColor(string token)
        {
            if(!colors.ContainsKey(token))
            {
                if(palette.Any())
                {
                    colors.Add(token, palette.Dequeue());
                }
                else
                {
                    colors.Add(token, "FFFFFF");
                }
            }

            return colors[token];
        }
    }
}


