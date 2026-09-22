import socket

UDP_IP = "192.168.11.196"
UDP_PORT = 11000
MESSAGE = """2,1,0,180"""

print ("UDP target IP:", UDP_IP)
print ("UDP target port:", UDP_PORT)
print ("message:", MESSAGE)

sock = socket.socket(socket.AF_INET, # Internet
                     socket.SOCK_DGRAM) # UDP
sock.sendto(MESSAGE.encode(), (UDP_IP, UDP_PORT))