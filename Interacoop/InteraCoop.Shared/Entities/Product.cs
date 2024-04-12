﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteraCoop.Shared.Entities
{
    public class Product
    {
        public int Id { get; set; }

        [Display(Name = "Tipo de producto")]
        public string ProductType { get; set; } = null!;

        [Display(Name = "Nombre de producto")]
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public string Name { get; set; } = null!;

        [Display(Name = "Cupo")]
        public int Quota { get; set; }

        [Display(Name = "Plazo")]
        public string? Term { get; set; }

        [Display(Name = "Valor")]
        public double? Value { get; set; }

        [Display(Name = "Tasa")]
        public double? Rate { get; set; }

        [Display(Name = "Audit Date")]
        [Required(ErrorMessage = "La {0} es requerida.")]
        public DateTime AuditDate { get; set; }

        [Display(Name = "Audit User")]
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public string AuditUser { get; set; } = null!;
    }
}