namespace ProjetoFilaDeJobs.DTOs;

// DTO de SAÍDA (resposta) - o contraponto do CriarPedidoRequest, que é de ENTRADA.
// A ideia é a mesma: nunca expor a entidade do EF Core diretamente na API.
//
// Repare que aqui NÃO existe uma propriedade "Pedido" dentro de ItemPedidoResponse -
// é justamente a ausência dessa referência de volta que elimina o ciclo
// que causou o erro "A possible object cycle was detected".
public record PedidoResponse(
    Guid Id,
    string ClienteNome,
    string ClienteEmail,
    string Status,
    decimal ValorTotal,
    DateTime CriadoEm,
    List<ItemPedidoResponse> Itens
);

public record ItemPedidoResponse(
    Guid Id,
    string ProdutoNome,
    int Quantidade,
    decimal PrecoUnitario
);