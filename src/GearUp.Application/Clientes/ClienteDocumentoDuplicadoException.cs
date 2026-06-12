namespace GearUp.Application.Clientes;

public sealed class ClienteDocumentoDuplicadoException(string documento)
    : InvalidOperationException(
        $"Já existe um cliente cadastrado com o documento {documento}.");
