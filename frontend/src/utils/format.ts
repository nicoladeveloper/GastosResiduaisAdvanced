/** Formata um número como moeda brasileira (R$ 1.234,56). */
export function formatarMoeda(valor: number): string {
  return valor.toLocaleString("pt-BR", {
    style: "currency",
    currency: "BRL",
  });
}
