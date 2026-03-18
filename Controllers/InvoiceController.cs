using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SarEquipEnterprise.Models;
using SarEquipEnterprise.Services;

namespace SarEquipEnterprise.Controllers
{
    [Authorize]
    public class InvoiceController : Controller
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            _invoiceService = invoiceService;
        }

        public async Task<IActionResult> Index()
        {
            var invoices = await _invoiceService.GetAllInvoicesAsync();
            var activeInvoices = invoices.Where(i => i.Status != "Archived").ToList();
            return View(activeInvoices);
        }

        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
            if (invoice == null)
                return NotFound();

            return View(invoice);
        }

        public async Task<IActionResult> Create()
        {
            var invoice = new Invoice
            {
                InvoiceDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(30),
                Items = new List<InvoiceItem>()
            };

            var items = await _invoiceService.GetItemsAsync();

            ViewBag.Items = items
                .Select(i => new SelectListItem
                {
                    Value = i.Name, // Using Name as value for Description compatibility
                    Text = i.Name
                }).ToList();

            ViewBag.ItemsFull = items;

            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Invoice invoice)
        {
            if (ModelState.IsValid)
            {
                await _invoiceService.CreateInvoiceAsync(invoice);
                return RedirectToAction(nameof(Index));
            }

            var items = await _invoiceService.GetItemsAsync();
            ViewBag.ItemsFull = items;
            ViewBag.Items = items.Select(i => new SelectListItem { Value = i.Name, Text = i.Name }).ToList();

            return View(invoice);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
            if (invoice == null)
                return NotFound();

            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Invoice invoice)
        {
            if (id != invoice.InvoiceId)
                return NotFound();

            if (ModelState.IsValid)
            {
                await _invoiceService.UpdateInvoiceAsync(invoice);
                return RedirectToAction(nameof(Index));
            }
            return View(invoice);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
            if (invoice == null)
                return NotFound();

            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _invoiceService.DeleteInvoiceAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Send(int id)
        {
            await _invoiceService.SendInvoiceAsync(id);
            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> GeneratePdf(int id)
        {
            try
            {
                var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
                if (invoice == null)
                    return NotFound();

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        // HEADER
                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text("SarEquip Enterprise")
                                    .FontSize(22)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);

                                column.Item().Text("Equipment Supply & Services");
                                column.Item().Text("Davao City, Philippines");
                            });

                            row.ConstantItem(120)
                                .Height(60)
                                .Image("wwwroot/images/logos.png")
                                .FitArea();
                        });

                        // CONTENT
                        page.Content().PaddingVertical(20).Column(column =>
                        {
                            column.Spacing(20);

                            // INVOICE TITLE
                            column.Item().Text($"INVOICE #{invoice.InvoiceNumber}")
                                .FontSize(18)
                                .Bold();

                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Bill To").Bold();
                                    c.Item().Text(invoice.CustomerName);
                                    c.Item().Text(invoice.CustomerAddress);
                                    c.Item().Text(invoice.CustomerEmail);
                                });

                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text($"Invoice Date: {invoice.InvoiceDate:d}");
                                    c.Item().Text($"Due Date: {invoice.DueDate:d}");
                                });
                            });

                            // TABLE
                            column.Item().Element(container =>
                            {
                                container.Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(30);
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    // TABLE HEADER
                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("#").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Product").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Price").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Qty").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Total").Bold();
                                    });

                                    int index = 1;

                                    foreach (var item in invoice.Items)
                                    {
                                        table.Cell().Padding(5).Text(index++);
                                        table.Cell().Padding(5).Text(item.Description);
                                        table.Cell().Padding(5).AlignRight().Text($"₱{item.UnitPrice:N2}");
                                        table.Cell().Padding(5).AlignRight().Text(item.Quantity.ToString());
                                        table.Cell().Padding(5).AlignRight().Text($"₱{item.LineTotal:N2}");
                                    }
                                });
                            });

                            // TOTAL BOX
                            column.Item().AlignRight().Width(200).Border(1).Padding(10).Column(total =>
                            {
                                total.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Grand Total").Bold();
                                    row.RelativeItem().AlignRight().Text($"₱{invoice.TotalAmount:N2}").Bold();
                                });
                            });

                            // NOTES
                            if (!string.IsNullOrWhiteSpace(invoice.Notes))
                            {
                                column.Item().PaddingTop(20).Text($"Notes: {invoice.Notes}");
                            }
                        });

                        // FOOTER
                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                    });
                });

                var pdfStream = new MemoryStream();
                document.GeneratePdf(pdfStream);
                pdfStream.Position = 0;

                return File(pdfStream, "application/pdf", $"Invoice_{invoice.InvoiceNumber}.pdf");
            }
            catch (Exception ex)
            {
                return Content($"Error generating PDF:\n{ex.Message}\n\n{ex.InnerException?.Message}", "text/plain");
            }
        }

        private class AddressComponent : IComponent
        {
            private string Title { get; }
            private string CompanyName { get; }
            private string Address { get; }

            public AddressComponent(string title, string companyName, string address)
            {
                Title = title;
                CompanyName = companyName;
                Address = address;
            }

            public void Compose(IContainer container)
            {
                container.Column(column =>
                {
                    column.Spacing(2);
                    column.Item().BorderBottom(1).PaddingBottom(5).Text(Title).SemiBold();
                    column.Item().Text(CompanyName).SemiBold();
                    column.Item().Text(Address);
                });
            }
        }
    }
}
