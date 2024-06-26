﻿using InteraCoop.Shared.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteraCoop.Shared.Dtos
{
    public class InteractionDto
    {
        public int Id
        { get; set; }

        [Display(Name = "Tipo de interacción")]
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [MaxLength(30, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
        public string InteractionType
        { get; set; } = null!;

        [Display(Name = "Fecha de creación de la interacción")]
        [DataType(DataType.Date)]
        public DateTime InteractionCreationDate
        { get; set; }

        [Display(Name = "Fecha de inicio")]
        [DataType(DataType.Date)]
        public DateTime StartDate
        { get; set; }

        [Display(Name = "Fecha fin")]
        [DataType(DataType.Date)]
        public DateTime EndDate
        { get; set; }

        [Display(Name = "Dirección")]
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
        public string Address
        { get; set; } = null!;

        [Display(Name = " Observaciones de la interacción")]
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [MaxLength(250, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
        public string ObservationsInteraction
        { get; set; } = null!;

        [Display(Name = "Oficina")]
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
        public string Office
        { get; set; } = null!;

        [Display(Name = "Fecha de auditoría")]
        [DataType(DataType.Date)]
        public DateTime AuditDate
        { get; set; }

        public int ClientId { get; set; }
        public string UserDocument { get; set; } = null!;

        public string? clientName;
        public int? Document { get; set; }
    }
}
