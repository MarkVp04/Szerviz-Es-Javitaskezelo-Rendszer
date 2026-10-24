using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Api.Controllers;

[ApiController, Authorize]
public abstract class SzervizControllerBase : ControllerBase
{
    protected const string Munkatarsak = Szerepkorok.Munkatarsak;
    protected const string Recepcio = Szerepkorok.Ugyfelszolgalat;
    protected const string Technikus = Szerepkorok.Muszaki;
}
