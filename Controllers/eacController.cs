using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeNET.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class eacController : ControllerBase
    {
        private bool enable2022 = false;
        private bool enable2021 = true;

        [HttpGet("challenge")]
        public IActionResult getChal()
        {
            if (enable2021)
            {
                return Ok("\"9dcc9707-e722-4126-b1f8-9dc45a3a6605\"");
            }

            if (enable2022)
            {
                return Ok("\"AQAAAHsg7mW5FQEE9HVl9EKMWXrqDzQxUCdgV/IPuQfbRgTx+cGnQqhhAgv1RvpihEC77gQ29JdoGFn2806Q+QPEj7nYg9C8pynbaiSVO8rKLJPvROsHuSXVJpQMv3TD8KyK3Y+n5bb86vAb5kRdZGD//uC8HY+D9jJLlEfTUlU=\"");
            }

            return Ok("\"AQAAAGd9O3h2ynQW6Y/1MhdZC8VoHygxyTzmiRvAfpiRtJEBQ+NVaXMStRTsYQk42H1hbB7NGKhIpgfShk+ADtRW9EU/YF5320eGmINJZAqkm3pyX9QF/w1QT4IB2EQfOqTfpryQ3QFchXYqDgg/SbX+/X9mgzssbflIw3OAK+c=\"");
        }
    }
}
