﻿using AutoMapper;
using MovieTheaterAPI.DTOs;
using MovieTheaterAPI.Entities;
using MovieTheaterAPI.Repository;
using MovieTheaterAPI.Services.Interfaces;

namespace MovieTheaterAPI.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ScheduleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ScheduleDTO> CreateSchedule(ScheduleDTO schedule)
        {
            var newSchedule = _mapper.Map<Schedule>(schedule);
            var movie = await _unitOfWork.MovieRepository.GetById(schedule.MovieId);
            var length = movie.Length;
            int startMinutes = newSchedule.StartTime?.Hour * 60 + newSchedule.StartTime?.Minute ?? 0;
            int endMinutes = startMinutes + length + 15;
            int endHour = endMinutes / 60;
            int endMinute = endMinutes % 60;
            var endTime = new TimeOnly(endHour, endMinute);
            newSchedule.EndTime = endTime;

            var existingSchedules = await _unitOfWork.ScheduleRepository.GetSchedulesByDateAndRoom(newSchedule.RoomId, newSchedule.ScheduleDate);
            foreach (var existingSchedule in existingSchedules)
            {
                if (newSchedule.StartTime >= existingSchedule.StartTime && newSchedule.StartTime < existingSchedule.EndTime ||
                    newSchedule.EndTime > existingSchedule.StartTime && newSchedule.EndTime <= existingSchedule.EndTime)
                {
                    throw new Exception("Schedule is conflicted with existing schedules");
                }
            }

            await _unitOfWork.ScheduleRepository.Add(newSchedule);
            await _unitOfWork.Save();

            var seats = await _unitOfWork.SeatRepository.GetSeatsByRoomId(schedule.RoomId);

            foreach (var seat in seats)
            {
                var ticket = new Ticket
                {
                    ScheduleId = newSchedule.Id,
                    SeatId = seat.Id,
                    FinalPrice = seat.SeatType.Price,
                    status = 0
                };
                await _unitOfWork.TicketRepository.Add(ticket);
                await _unitOfWork.Save();
            }
            return schedule;
        }

        public async Task DeleteSchedule(int id)
        {
            var schedule = await _unitOfWork.ScheduleRepository.GetById(id);
            await _unitOfWork.ScheduleRepository.Delete(schedule);
            await _unitOfWork.Save();
        }

        public async Task<IEnumerable<ScheduleDTO>> GetAllSchedules()
        {
            var schedules = await _unitOfWork.ScheduleRepository.GetAll();
            return _mapper.Map<IEnumerable<ScheduleDTO>>(schedules);
        }

        public async Task<ScheduleDTO> GetScheduleById(int id)
        {
            var schedule = await _unitOfWork.ScheduleRepository.GetById(id);
            return _mapper.Map<ScheduleDTO>(schedule);
        }

        public async Task<IEnumerable<ScheduleDTO>> GetSchedulesByMovie(int movieId)
        {
            var schedules = await _unitOfWork.ScheduleRepository.GetSchedulesByMovie(movieId);
            return _mapper.Map<IEnumerable<ScheduleDTO>>(schedules);
        }

        public async Task UpdateSchedule(int id, ScheduleDTO scheduleDTO)
        {
            if (id != scheduleDTO.Id)
            {
                throw new Exception("Id is not matching");
            }
            var schedule = _mapper.Map<Schedule>(scheduleDTO);
            await _unitOfWork.ScheduleRepository.Update(schedule);
            await _unitOfWork.Save();
        }
    }
}