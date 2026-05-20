using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Controllers;
using LibraryManagement.Data;

namespace LibraryManagement.Tests
{
    [TestClass]
    public class BooksControllerTests
    {
        [TestMethod]
        public async Task Index_ReturnsViewResult()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;

            using var context = new ApplicationDbContext(options);

            var controller = new BooksController(context);

            // Act
            var result = await controller.Index();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }
    }
}