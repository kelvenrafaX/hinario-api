using System.Text;
using MinhaPrimeiraApi.Models;

namespace MinhaPrimeiraApi.Utils
{
    /// <summary>
    /// Monta o trecho (preview) para resultado de pesquisa: primeira linha + linha onde o termo foi encontrado,
    /// com o termo em negrito (html <b>termo</b>).
    /// </summary>
    public static class TrechoPesquisaHelper
    {
        private const string NegritoMarcadorInicial = "<b>";
        private const string NegritoMarcadorFinal = "</b>";

        /// <summary>
        /// Gera o trecho para exibição na pesquisa.
        /// Regras: no máximo primeira linha + linha onde a palavra foi encontrada;
        /// se a palavra estiver na primeira linha, retorna as 2 primeiras linhas;
        /// a palavra encontrada é retornada em negrito (<b>palavra</b>).
        /// </summary>
        public static string BuildTrecho(Hino hino, string[] palavrasNormalizadas)
        {
            if (string.IsNullOrWhiteSpace(hino.Letra) || palavrasNormalizadas.Length == 0)
                return string.Empty;

            var linhas = hino.Letra.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToArray();
            if (linhas.Length == 0)
                return string.Empty;

            // Primeira e segunda linha sempre entra
            var primeirasLinhas = linhas[0];
            if (linhas.Length > 1)
                primeirasLinhas += '\n' + linhas[1];

            // Descobrir em qual linha (índice) alguma palavra foi encontrada
            int indiceLinhaEncontrada = -1;
            string? palavraEncontrada = null;

            for (int i = 0; i < linhas.Length; i++)
            {
                var linhaNorm = TextNormalizer.Normalizar(linhas[i]);
                foreach (var palavra in palavrasNormalizadas)
                {
                    if (string.IsNullOrEmpty(palavra)) continue;
                    if (linhaNorm.Contains(palavra))
                    {
                        indiceLinhaEncontrada = i;
                        palavraEncontrada = palavra;
                        break;
                    }
                }
                if (indiceLinhaEncontrada >= 0) break;
            }

            // Se não encontrou em nenhuma linha (não deveria acontecer se veio do pesquisar), retorna só a primeira
            if (indiceLinhaEncontrada < 0 || string.IsNullOrEmpty(palavraEncontrada))
                return ColocarTermoEmNegrito(primeirasLinhas, primeirasLinhas, palavrasNormalizadas);

            if (indiceLinhaEncontrada == 0)
            {
                // Termo na primeira linha: retornar as 2 primeiras linhas (se houver)
                var linhasPreview = linhas.Length >= 2
                    ? new[] { linhas[0], linhas[1] }
                    : new[] { linhas[0] };
                var resultado = new List<string>();
                foreach (var linha in linhasPreview)
                    resultado.Add(ColocarTermoEmNegrito(linha, linha, palavrasNormalizadas));
                return string.Join("\n", resultado);
            }

            // Termo em outra linha: primeiras linhas + linha onde encontrou
            var primeiraComNegrito = ColocarTermoEmNegrito(primeirasLinhas, primeirasLinhas, palavrasNormalizadas);
            var linhaEncontradaComNegrito = ColocarTermoEmNegrito(linhas[indiceLinhaEncontrada], linhas[indiceLinhaEncontrada], palavrasNormalizadas);
            if (linhas.Length > indiceLinhaEncontrada)
            {
                linhaEncontradaComNegrito += "\n" + ColocarTermoEmNegrito(linhas[indiceLinhaEncontrada+1], linhas[indiceLinhaEncontrada+1], palavrasNormalizadas);
            }
            return primeiraComNegrito + "..." + " \n " + "..." + linhaEncontradaComNegrito;
        }

        /// <summary>
        /// Coloca em negrito (**) a primeira ocorrência de qualquer palavra normalizada que exista na linha original.
        /// Usa mapeamento normalizado -> original para destacar o texto original (com acentos).
        /// </summary>
        private static string ColocarTermoEmNegrito(string linhaOriginal, string linhaParaBuscar, string[] palavrasNormalizadas)
        {
            var linhaNorm = TextNormalizer.Normalizar(linhaParaBuscar);
            (int start, int len)? segmentoNorm = null;
            string? palavraMatch = null;

            foreach (var palavra in palavrasNormalizadas)
            {
                if (string.IsNullOrEmpty(palavra)) continue;
                var idx = linhaNorm.IndexOf(palavra, StringComparison.Ordinal);
                if (idx >= 0)
                {
                    segmentoNorm = (idx, palavra.Length);
                    palavraMatch = palavra;
                    break;
                }
            }

            if (!segmentoNorm.HasValue || string.IsNullOrEmpty(palavraMatch))
                return linhaOriginal;

            var (normStart, normLen) = segmentoNorm.Value;

            // Mapear posições normalizadas de volta para a linha original (para acentos: "água" vs "agua")
            var normToOrig = new List<int>();
            var normSb = new StringBuilder();
            for (int i = 0; i < linhaOriginal.Length; i++)
            {
                var n = TextNormalizer.Normalizar(linhaOriginal[i].ToString());
                for (int k = 0; k < n.Length; k++)
                {
                    normToOrig.Add(i);
                    normSb.Append(n[k]);
                }
            }

            if (normStart + normLen > normToOrig.Count)
                return linhaOriginal;

            int origStart = normToOrig[normStart];
            int origEnd = normToOrig[normStart + normLen - 1] + 1;
            if (origEnd > linhaOriginal.Length) origEnd = linhaOriginal.Length;

            var termoOriginal = linhaOriginal.Substring(origStart, origEnd - origStart);
            return linhaOriginal.Substring(0, origStart)
                 + NegritoMarcadorInicial + termoOriginal + NegritoMarcadorFinal
                 + linhaOriginal.Substring(origEnd);
        }
    }
}
