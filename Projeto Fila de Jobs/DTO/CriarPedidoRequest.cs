namespace ProjetoFilaDeJobs.DTOs;

// record com sintaxe posicional: os parâmetros já viram propriedades
// imutáveis (init-only) automaticamente - não precisamos escrever
// "{ get; set; }" para cada campo como faríamos numa class.
//
// Igualdade por valor: dois CriarPedidoRequest com os mesmos dados
// são considerados "==", o que é útil em testes e comparações.
public record CriarPedidoRequest(
    string ClienteNome,
    string ClienteEmail,
    List<ItemPedidoRequest> Itens
);

// Mesmo raciocínio para o item do pedido - é só um "pacote de dados"
// que chega na requisição, sem nenhum comportamento próprio.
public record ItemPedidoRequest(
    string ProdutoNome,
    int Quantidade,
    decimal PrecoUnitario
);