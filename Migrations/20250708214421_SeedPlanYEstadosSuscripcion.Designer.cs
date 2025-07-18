﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using webapi;

#nullable disable

namespace webapi.Migrations
{
    [DbContext(typeof(CoachPrimeContext))]
    [Migration("20250708214421_SeedPlanYEstadosSuscripcion")]
    partial class SeedPlanYEstadosSuscripcion
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Agrupacion", b =>
                {
                    b.Property<int>("AgrupacionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AgrupacionId"));

                    b.Property<int>("DiaEntrenamientoId")
                        .HasColumnType("int");

                    b.Property<string>("Tipo")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("AgrupacionId");

                    b.HasIndex("DiaEntrenamientoId");

                    b.ToTable("Agrupaciones");
                });

            modelBuilder.Entity("Alimento", b =>
                {
                    b.Property<int>("AlimentoId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AlimentoId"));

                    b.Property<double>("Cantidad")
                        .HasColumnType("float");

                    b.Property<int>("ComidaId")
                        .HasColumnType("int");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Unidad")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("AlimentoId");

                    b.HasIndex("ComidaId");

                    b.ToTable("Alimentos");
                });

            modelBuilder.Entity("Cliente", b =>
                {
                    b.Property<int>("ClienteId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ClienteId"));

                    b.Property<string>("Apellido")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("FechaNacimiento")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("FechaRegistro")
                        .HasColumnType("datetime2");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Sexo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Telefono")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UsuarioId")
                        .HasColumnType("int");

                    b.HasKey("ClienteId");

                    b.HasIndex("UsuarioId");

                    b.ToTable("Clientes");
                });

            modelBuilder.Entity("Comida", b =>
                {
                    b.Property<int>("ComidaId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ComidaId"));

                    b.Property<int>("DietaId")
                        .HasColumnType("int");

                    b.Property<string>("Hora")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ComidaId");

                    b.HasIndex("DietaId");

                    b.ToTable("Comidas");
                });

            modelBuilder.Entity("DiaEntrenamiento", b =>
                {
                    b.Property<int>("DiaEntrenamientoId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("DiaEntrenamientoId"));

                    b.Property<string>("DiaSemana")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RutinaId")
                        .HasColumnType("int");

                    b.HasKey("DiaEntrenamientoId");

                    b.HasIndex("RutinaId");

                    b.ToTable("DiasEntrenamiento");
                });

            modelBuilder.Entity("Dieta", b =>
                {
                    b.Property<int>("DietaId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("DietaId"));

                    b.Property<int>("ClienteId")
                        .HasColumnType("int");

                    b.Property<DateTime>("FechaAsignacion")
                        .HasColumnType("datetime2");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Notas")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("DietaId");

                    b.HasIndex("ClienteId");

                    b.ToTable("Dietas");
                });

            modelBuilder.Entity("Ejercicio", b =>
                {
                    b.Property<int>("EjercicioId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("EjercicioId"));

                    b.Property<string>("Descripcion")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ImagenKey")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Repeticiones")
                        .HasColumnType("int");

                    b.Property<int>("Series")
                        .HasColumnType("int");

                    b.HasKey("EjercicioId");

                    b.ToTable("Ejercicios");
                });

            modelBuilder.Entity("EjercicioAgrupado", b =>
                {
                    b.Property<int>("EjercicioAgrupadoId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("EjercicioAgrupadoId"));

                    b.Property<int>("AgrupacionId")
                        .HasColumnType("int");

                    b.Property<int>("EjercicioId")
                        .HasColumnType("int");

                    b.HasKey("EjercicioAgrupadoId");

                    b.HasIndex("AgrupacionId");

                    b.HasIndex("EjercicioId");

                    b.ToTable("EjercicioAgrupado");
                });

            modelBuilder.Entity("EstadoSuscripcion", b =>
                {
                    b.Property<int>("EstadoSuscripcionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("EstadoSuscripcionId"));

                    b.Property<string>("Descripcion")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("EsFinal")
                        .HasColumnType("bit");

                    b.Property<string>("NombreEstado")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("EstadoSuscripcionId");

                    b.ToTable("EstadoSuscripcion");

                    b.HasData(
                        new
                        {
                            EstadoSuscripcionId = 1,
                            Descripcion = "Pago iniciado pero no completado.",
                            EsFinal = false,
                            NombreEstado = "Pendiente"
                        },
                        new
                        {
                            EstadoSuscripcionId = 2,
                            Descripcion = "Suscripción activa y en buen estado.",
                            EsFinal = false,
                            NombreEstado = "Activa"
                        },
                        new
                        {
                            EstadoSuscripcionId = 3,
                            Descripcion = "Periodo de suscripción finalizado.",
                            EsFinal = true,
                            NombreEstado = "Expirada"
                        },
                        new
                        {
                            EstadoSuscripcionId = 4,
                            Descripcion = "Usuario canceló la suscripción.",
                            EsFinal = true,
                            NombreEstado = "Cancelada"
                        },
                        new
                        {
                            EstadoSuscripcionId = 5,
                            Descripcion = "Periodo de suspensión/cancelación.",
                            EsFinal = false,
                            NombreEstado = "Suspendida"
                        },
                        new
                        {
                            EstadoSuscripcionId = 6,
                            Descripcion = "Reactivada después de suspensión/cancelación.",
                            EsFinal = false,
                            NombreEstado = "Reactivada"
                        },
                        new
                        {
                            EstadoSuscripcionId = 7,
                            Descripcion = "Periodo de prueba gratuita.",
                            EsFinal = false,
                            NombreEstado = "Prueba"
                        });
                });

            modelBuilder.Entity("Plan", b =>
                {
                    b.Property<int>("PlanId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PlanId"));

                    b.Property<string>("Beneficios")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Estado")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Frecuencia")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("MaxClientes")
                        .HasColumnType("int");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Precio")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("StripePriceId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("PlanId");

                    b.ToTable("Plan", (string)null);

                    b.HasData(
                        new
                        {
                            PlanId = 1,
                            Beneficios = "Acceso básico a la app: Sin reportes avanzados.",
                            Estado = "Activo",
                            Frecuencia = "Mensual",
                            MaxClientes = 5,
                            Nombre = "Básico",
                            Precio = 0.00m
                        },
                        new
                        {
                            PlanId = 2,
                            Beneficios = "Reportes avanzados: Soporte dedicado; Integración avanzada.",
                            Estado = "Activo",
                            Frecuencia = "Mensual",
                            Nombre = "Premium",
                            Precio = 499.00m,
                            StripePriceId = "price_1Q8KUQBZAdKpouIVDKjLz25"
                        },
                        new
                        {
                            PlanId = 4,
                            Beneficios = "Igual que Premium; Descuento anual.",
                            Estado = "Activo",
                            Frecuencia = "Anual",
                            Nombre = "Anual Premium",
                            Precio = 4999.00m,
                            StripePriceId = "price_1Q9r7hBZAdKpouIVK5WRxMl"
                        });
                });

            modelBuilder.Entity("Progreso", b =>
                {
                    b.Property<int>("ProgresoId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ProgresoId"));

                    b.Property<double?>("BrazoCm")
                        .HasColumnType("float");

                    b.Property<double?>("CaderaCm")
                        .HasColumnType("float");

                    b.Property<double>("CaloriasDiarias")
                        .HasColumnType("float");

                    b.Property<double?>("CinturaCm")
                        .HasColumnType("float");

                    b.Property<int>("ClienteId")
                        .HasColumnType("int");

                    b.Property<double>("EstaturaCm")
                        .HasColumnType("float");

                    b.Property<double>("FactorActividad")
                        .HasColumnType("float");

                    b.Property<DateTime>("FechaRegistro")
                        .HasColumnType("datetime2");

                    b.Property<double>("IMC")
                        .HasColumnType("float");

                    b.Property<string>("NivelActividad")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Notas")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("PechoCm")
                        .HasColumnType("float");

                    b.Property<double>("PesoKg")
                        .HasColumnType("float");

                    b.Property<double?>("PiernaCm")
                        .HasColumnType("float");

                    b.Property<double>("TMB")
                        .HasColumnType("float");

                    b.HasKey("ProgresoId");

                    b.HasIndex("ClienteId");

                    b.ToTable("Progresos");
                });

            modelBuilder.Entity("RefreshToken", b =>
                {
                    b.Property<int>("RefreshTokenId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RefreshTokenId"));

                    b.Property<DateTime>("ExpirationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UsuarioId")
                        .HasColumnType("int");

                    b.HasKey("RefreshTokenId");

                    b.HasIndex("UsuarioId");

                    b.ToTable("RefreshTokens");
                });

            modelBuilder.Entity("Rutina", b =>
                {
                    b.Property<int>("RutinaId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RutinaId"));

                    b.Property<int>("ClienteId")
                        .HasColumnType("int");

                    b.Property<string>("Descripcion")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("FechaFin")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("FechaInicio")
                        .HasColumnType("datetime2");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UsuarioId")
                        .HasColumnType("int");

                    b.HasKey("RutinaId");

                    b.HasIndex("ClienteId");

                    b.HasIndex("UsuarioId");

                    b.ToTable("Rutinas");
                });

            modelBuilder.Entity("Suscripcion", b =>
                {
                    b.Property<int>("SuscripcionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SuscripcionId"));

                    b.Property<int>("EstadoSuscripcionId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("FechaCancelacion")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("FechaFin")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("FechaInicio")
                        .HasColumnType("datetime2");

                    b.Property<int>("PlanId")
                        .HasColumnType("int");

                    b.Property<string>("StripeSubscriptionId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UsuarioId")
                        .HasColumnType("int");

                    b.HasKey("SuscripcionId");

                    b.HasIndex("EstadoSuscripcionId");

                    b.HasIndex("PlanId");

                    b.HasIndex("UsuarioId");

                    b.ToTable("Suscripcion", (string)null);
                });

            modelBuilder.Entity("Usuario", b =>
                {
                    b.Property<int>("UsuarioId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UsuarioId"));

                    b.Property<string>("Apellido")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("EmailVerificado")
                        .HasColumnType("bit");

                    b.Property<string>("EmailVerificationToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("FechaRegistro")
                        .HasColumnType("datetime2");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PasswordResetToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Rol")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("TokenExpirationDate")
                        .HasColumnType("datetime2");

                    b.HasKey("UsuarioId");

                    b.ToTable("Usuarios");
                });

            modelBuilder.Entity("Agrupacion", b =>
                {
                    b.HasOne("DiaEntrenamiento", "DiaEntrenamiento")
                        .WithMany("Agrupaciones")
                        .HasForeignKey("DiaEntrenamientoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DiaEntrenamiento");
                });

            modelBuilder.Entity("Alimento", b =>
                {
                    b.HasOne("Comida", "Comida")
                        .WithMany("Alimentos")
                        .HasForeignKey("ComidaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Comida");
                });

            modelBuilder.Entity("Cliente", b =>
                {
                    b.HasOne("Usuario", "Usuario")
                        .WithMany("Clientes")
                        .HasForeignKey("UsuarioId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Usuario");
                });

            modelBuilder.Entity("Comida", b =>
                {
                    b.HasOne("Dieta", "Dieta")
                        .WithMany("Comidas")
                        .HasForeignKey("DietaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Dieta");
                });

            modelBuilder.Entity("DiaEntrenamiento", b =>
                {
                    b.HasOne("Rutina", "Rutina")
                        .WithMany("DiasEntrenamiento")
                        .HasForeignKey("RutinaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Rutina");
                });

            modelBuilder.Entity("Dieta", b =>
                {
                    b.HasOne("Cliente", "Cliente")
                        .WithMany("Dietas")
                        .HasForeignKey("ClienteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Cliente");
                });

            modelBuilder.Entity("EjercicioAgrupado", b =>
                {
                    b.HasOne("Agrupacion", "Agrupacion")
                        .WithMany("EjerciciosAgrupados")
                        .HasForeignKey("AgrupacionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Ejercicio", "Ejercicio")
                        .WithMany()
                        .HasForeignKey("EjercicioId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Agrupacion");

                    b.Navigation("Ejercicio");
                });

            modelBuilder.Entity("Progreso", b =>
                {
                    b.HasOne("Cliente", "Cliente")
                        .WithMany("Progresos")
                        .HasForeignKey("ClienteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Cliente");
                });

            modelBuilder.Entity("RefreshToken", b =>
                {
                    b.HasOne("Usuario", "Usuario")
                        .WithMany("RefreshTokens")
                        .HasForeignKey("UsuarioId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Usuario");
                });

            modelBuilder.Entity("Rutina", b =>
                {
                    b.HasOne("Cliente", "Cliente")
                        .WithMany("Rutinas")
                        .HasForeignKey("ClienteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Usuario", "Usuario")
                        .WithMany()
                        .HasForeignKey("UsuarioId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Cliente");

                    b.Navigation("Usuario");
                });

            modelBuilder.Entity("Suscripcion", b =>
                {
                    b.HasOne("EstadoSuscripcion", "EstadoSuscripcion")
                        .WithMany()
                        .HasForeignKey("EstadoSuscripcionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Plan", "Plan")
                        .WithMany()
                        .HasForeignKey("PlanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Usuario", "Usuario")
                        .WithMany("Suscripciones")
                        .HasForeignKey("UsuarioId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EstadoSuscripcion");

                    b.Navigation("Plan");

                    b.Navigation("Usuario");
                });

            modelBuilder.Entity("Agrupacion", b =>
                {
                    b.Navigation("EjerciciosAgrupados");
                });

            modelBuilder.Entity("Cliente", b =>
                {
                    b.Navigation("Dietas");

                    b.Navigation("Progresos");

                    b.Navigation("Rutinas");
                });

            modelBuilder.Entity("Comida", b =>
                {
                    b.Navigation("Alimentos");
                });

            modelBuilder.Entity("DiaEntrenamiento", b =>
                {
                    b.Navigation("Agrupaciones");
                });

            modelBuilder.Entity("Dieta", b =>
                {
                    b.Navigation("Comidas");
                });

            modelBuilder.Entity("Rutina", b =>
                {
                    b.Navigation("DiasEntrenamiento");
                });

            modelBuilder.Entity("Usuario", b =>
                {
                    b.Navigation("Clientes");

                    b.Navigation("RefreshTokens");

                    b.Navigation("Suscripciones");
                });
#pragma warning restore 612, 618
        }
    }
}
