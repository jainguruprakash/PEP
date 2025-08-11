@echo off
echo Testing PEP Scanner Scrapers and Jobs
echo ====================================

echo.
echo 1. Testing individual scrapers via API endpoints:
echo.

echo Testing RBI scraper...
curl -X POST "http://localhost:5098/api/publicdata/scrape/rbi" -H "Content-Type: application/json"
echo.

echo Testing SEBI scraper...
curl -X POST "http://localhost:5098/api/publicdata/scrape/sebi" -H "Content-Type: application/json"
echo.

echo Testing Parliament scraper...
curl -X POST "http://localhost:5098/api/publicdata/scrape/parliament" -H "Content-Type: application/json"
echo.

echo Testing Wikipedia scraper...
curl -X POST "http://localhost:5098/api/publicdata/scrape/wikipedia" -H "Content-Type: application/json"
echo.

echo Testing OpenSanctions scraper...
curl -X POST "http://localhost:5098/api/publicdata/scrape/opensanctions" -H "Content-Type: application/json"
echo.

echo Testing all scrapers at once...
curl -X POST "http://localhost:5098/api/publicdata/scrape/all" -H "Content-Type: application/json"
echo.

echo.
echo 2. Testing watchlist import endpoints:
echo.

echo Testing OFAC import...
curl -X POST "http://localhost:5098/api/watchlistimport/ofac" -H "Content-Type: application/json"
echo.

echo Testing UN import...
curl -X POST "http://localhost:5098/api/watchlistimport/un" -H "Content-Type: application/json"
echo.

echo.
echo 3. Checking Hangfire dashboard:
echo Open http://localhost:5098/hangfire in your browser to see job status
echo.

echo.
echo 4. Checking watchlist update status:
echo.
curl -X GET "http://localhost:5098/api/watchlistupdate/status" -H "Content-Type: application/json"
echo.

echo.
echo Test completed! Check the responses above for results.
pause