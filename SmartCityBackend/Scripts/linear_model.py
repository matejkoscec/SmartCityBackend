import psycopg2
import numpy as np
from sklearn.preprocessing import PolynomialFeatures
from sklearn.linear_model import LinearRegression

# Replace these values with your database connection details
db_config = {
    'dbname': 'backend',
    'user': 'backend',
    'password': 'backend',
    'host': 'localhost',
}

try:
    conn = psycopg2.connect(**db_config)
except psycopg2.Error as e:
    print("Error: Unable to connect to the database")
    print(e)

try:
    cursor = conn.cursor()

    # Execute your SQL query for todays history
    cursor.execute("SELECT zone, price, start_time FROM parking_spot_history JOIN zone_price ON parking_spot_history.zone_price_id = zone_price.id WHERE parking_spot_history.is_occupied IS false AND parking_spot_history.start_time >= NOW() - interval '24 hours';")
    rows = cursor.fetchall()

    # Expected values for the prices per zone
    zone_prices = { 
        1: 2.00,
        2: 1.50,
        3: 1.00,
        4: 0.50
    }
    
    # Sort data by the zones
    data_per_zone = {i: [] for i in zone_prices.keys()}
    prices_per_zone_per_hour = {i: {j: zone_prices[i] for j in range(0, 24)} for i in zone_prices.keys()}
    
    for row in rows:   
        data_per_zone[row[0]].append([float(row[1]), row[2].hour])
 
    # Sort data by the hours of the day, calculate the average price per hour, predict the prices for all hours for the next day
    for row in data_per_zone:
        
        reservations_per_hour = {i: 0 for i in range(0, 24)}
        for reservation in data_per_zone[row]:
            reservations_per_hour[reservation[1]] += 1

        # Calculate expected number of reservations per hour
        expected_reservations_per_hour = sum(reservations_per_hour.values()) / 24
        
        # Calculate the standard deviation
        standard_deviation = np.std(list(reservations_per_hour.values()))
    
        # Calculate the expected number of reservations per hour
        # based on the linear regression
        X = np.array(list(reservations_per_hour.keys())).reshape(-1, 1)
        phi = PolynomialFeatures(degree=3, include_bias=False).fit_transform(X)
        y = np.array(list(reservations_per_hour.values())).reshape(-1, 1)
        regression = LinearRegression().fit(phi, y)

        expected_reservations_per_hour_regression = regression.predict(phi)

        last_price = zone_prices[row]
        if expected_reservations_per_hour != 0:
            for i in range(0, 24):
                # Calculate the price based on the expected number of reservations per hour
                # and the standard deviation
                prices_per_zone_per_hour[row][i] = np.round(last_price + (expected_reservations_per_hour_regression[i] - expected_reservations_per_hour) / expected_reservations_per_hour, 2)    
                if prices_per_zone_per_hour[row][i] < last_price:
                    prices_per_zone_per_hour[row][i] = last_price
            
    for zone in prices_per_zone_per_hour:
        for hour in prices_per_zone_per_hour[zone]:
            cursor.execute("INSERT INTO zone_price (zone, price, created_at_utc) VALUES (%s, %s, NOW());", (zone, float(prices_per_zone_per_hour[zone][hour])))
            conn.commit()

except psycopg2.Error as e:
    print("Error: Unable to execute the SQL query")
    print(e)
finally:
    # Close the cursor and the database connection
    cursor.close()
    conn.close()