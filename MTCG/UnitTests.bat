@echo off
echo Running Monster Trading Card Game Tests...

REM Test 1: Register Users
echo Testing user registration...
curl -X POST http://localhost:10001/users -H "Content-Type: application/json" -d "{\"Username\":\"testuser\", \"Password\":\"password\"}"
curl -X POST http://localhost:10001/users -H "Content-Type: application/json" -d "{\"Username\":\"admin\", \"Password\":\"adminpass\"}"

REM Test 2: Login Users
echo Testing user login...
curl -X POST http://localhost:10001/sessions -H "Content-Type: application/json" -d "{\"Username\":\"testuser\", \"Password\":\"password\"}"
curl -X POST http://localhost:10001/sessions -H "Content-Type: application/json" -d "{\"Username\":\"admin\", \"Password\":\"adminpass\"}"

REM Test 3: Create Packages
echo Testing package creation...
curl -X POST http://localhost:10001/packages -H "Content-Type: application/json" -H "Authorization: Bearer admin-mtcgToken" -d "[{\"Id\":\"845f0dc7-37d0-426e-994e-43fc3ac83c08\", \"Name\":\"WaterGoblin\", \"Damage\": 10.0}, {\"Id\":\"644808c2-f87a-4600-b313-122b02322fd5\", \"Name\":\"FireElf\", \"Damage\": 20.0}, {\"Id\":\"99f8f8dc-e25e-4a95-aa2c-782823f36e2a\", \"Name\":\"Dragon\", \"Damage\": 50.0}, {\"Id\":\"4ec8b269-0dfa-4f97-809a-2c63fe2a0025\", \"Name\":\"Knight\", \"Damage\": 25.0}, {\"Id\":\"171f6076-4eb5-4a7d-b3f2-2d650cc3d237\", \"Name\":\"RegularSpell\", \"Damage\": 30.0}]"

REM Test 4: Buy Package
echo Testing package purchase...
curl -X POST http://localhost:10001/transactions/packages -H "Authorization: Bearer testuser-mtcgToken"

REM Test 5: Configure Deck
echo Testing deck configuration...
curl -X PUT http://localhost:10001/deck -H "Content-Type: application/json" -H "Authorization: Bearer testuser-mtcgToken" -d "[\"845f0dc7-37d0-426e-994e-43fc3ac83c08\", \"644808c2-f87a-4600-b313-122b02322fd5\", \"99f8f8dc-e25e-4a95-aa2c-782823f36e2a\", \"4ec8b269-0dfa-4f97-809a-2c63fe2a0025\"]"

REM Test 6: Get Stats
echo Testing stats retrieval...
curl -X GET http://localhost:10001/stats -H "Authorization: Bearer testuser-mtcgToken"

REM Test 7: Get Scoreboard
echo Testing scoreboard retrieval...
curl -X GET http://localhost:10001/scoreboard -H "Authorization: Bearer testuser-mtcgToken"

REM Test 8: Start Battle
echo Testing battle initiation...
curl -X POST http://localhost:10001/battles -H "Authorization: Bearer testuser-mtcgToken" & curl -X POST http://localhost:10001/battles -H "Authorization: Bearer admin-mtcgToken"

REM Test 9: Create Trade
echo Testing trade creation...
curl -X POST http://localhost:10001/tradings -H "Content-Type: application/json" -H "Authorization: Bearer testuser-mtcgToken" -d "{\"Id\":\"6cd85277-4590-49d4-b0cf-ba0a921faad0\", \"CardToTrade\":\"644808c2-f87a-4600-b313-122b02322fd5\", \"WantedType\":\"Dragon\", \"MinimumDamage\":20}"

REM Test 10: Get All Trades
echo Testing trade retrieval...
curl -X GET http://localhost:10001/tradings -H "Authorization: Bearer testuser-mtcgToken"

REM Test 11: Accept Trade
echo Testing trade acceptance...
curl -X POST http://localhost:10001/tradings/6cd85277-4590-49d4-b0cf-ba0a921faad0 -H "Authorization: Bearer admin-mtcgToken" -d "99f8f8dc-e25e-4a95-aa2c-782823f36e2a"

REM Test 12: Delete Trade
echo Testing trade deletion...
curl -X DELETE http://localhost:10001/tradings/6cd85277-4590-49d4-b0cf-ba0a921faad0 -H "Authorization: Bearer testuser-mtcgToken"

REM Test 13: Edit Profile
echo Testing profile editing...
curl -X PUT http://localhost:10001/users/testuser -H "Content-Type: application/json" -H "Authorization: Bearer testuser-mtcgToken" -d "{\"Name\":\"Test User\", \"Bio\":\"Enjoying the game!\", \"Image\":\":-)\"}"

REM Test 14: View Profile
echo Testing profile viewing...
curl -X GET http://localhost:10001/users/testuser -H "Authorization: Bearer testuser-mtcgToken"

REM Test 15: Unauthorized Request
echo Testing unauthorized access...
curl -X GET http://localhost:10001/deck

REM Test 16: Invalid Login
echo Testing invalid login...
curl -X POST http://localhost:10001/sessions -H "Content-Type: application/json" -d "{\"Username\":\"invaliduser\", \"Password\":\"wrongpass\"}"

REM Test 17: View Empty Deck
echo Testing empty deck view...
curl -X GET http://localhost:10001/deck -H "Authorization: Bearer testuser-mtcgToken"

REM Test 18: Get Cards
echo Testing cards retrieval...
curl -X GET http://localhost:10001/cards -H "Authorization: Bearer testuser-mtcgToken"

REM Test 19: Battle Without Deck
echo Testing battle with empty deck...
curl -X POST http://localhost:10001/battles -H "Authorization: Bearer testuser-mtcgToken"

REM Test 20: Invalid Package Creation
echo Testing invalid package creation...
curl -X POST http://localhost:10001/packages -H "Authorization: Bearer admin-mtcgToken" -H "Content-Type: application/json" -d "[{\"Id\":\"invalid-id\"}]"

echo All tests completed!
