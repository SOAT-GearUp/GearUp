# Infraestrutura AWS

Terraform para provisionar a infraestrutura AWS da Fase 2 do GearUp.

## Recursos

- VPC;
- subnets públicas, privadas e de banco;
- ECR;
- EKS;
- node group gerenciado;
- RDS PostgreSQL;
- security groups.

## Uso

```powershell
cd C:\Users\joseh\source\repos\GearUp\infra\aws
Copy-Item .\terraform.tfvars.example .\terraform.tfvars
terraform init
terraform plan
terraform apply
```

Se o ECR `gearup` já existir:

```powershell
terraform import aws_ecr_repository.gearup gearup
```

Documentação completa:

```text
docs/fase-2/Infraestrutura/Provisionamento com Terraform.md
```
