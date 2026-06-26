namespace GearUp.Application.Cadastro.Clientes.Exceptions;

public sealed class ClienteDocumentoDuplicadoException(string documento)
    : InvalidOperationException($"Já existe um cliente cadastrado com o documento {documento}.");
