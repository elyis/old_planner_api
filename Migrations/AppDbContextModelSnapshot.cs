﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using old_planner_api.src.Infrastructure.Data;

#nullable disable

namespace old_planner_api.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.11");

            modelBuilder.Entity("old_planner_api.src.Domain.Models.Board", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Boards");
                });

            modelBuilder.Entity("old_planner_api.src.Domain.Models.BoardMember", b =>
                {
                    b.Property<Guid>("BoardId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("BoardId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("BoardMembers");
                });

            modelBuilder.Entity("old_planner_api.src.Domain.Models.Chat", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Chats");
                });

            modelBuilder.Entity("old_planner_api.src.Domain.Models.ChatMembership", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("ChatId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateLastViewing")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "ChatId");

                    b.HasIndex("ChatId");

                    b.ToTable("ChatMemberships");
                });

            modelBuilder.Entity("old_planner_api.src.Domain.Models.ChatMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("ChatId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("SenderId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("SentAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ChatId");

                    b.HasIndex("SenderId");

                    b.ToTable("ChatMessages");
                });

            modelBuilder.Entity("old_planner_api.src.Domain.Models.DeletedTask", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ExistBeforeDate")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("TaskId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("TaskId")
                        .IsUnique();

                    b.ToTable("DeletedTasks");
                });

            modelBuilder.Entity("old_planner_api.src.Domain.Models.TaskChat", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Image")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("TaskId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("TaskId")
                        .IsUnique();

                    b.ToTable("TaskChats");
                });

            modelBuilder.Entity("old_planner_api.src.Domain.Models.TaskChatMembership", b =>
                {
                    b.Property<Guid>("ParticipantId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("ChatId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateLastViewing")
                        .HasColumnType("TEXT");

                    b.HasKey("ParticipantId", "ChatId");

                    b.HasIndex("ChatId");

                    b.ToTable("TaskChatMemberships");
                });

            modelBuilder.Entity("old_planner_api.src.Domain.Models.TaskChatMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("ChatId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAtDate")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("SenderId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ChatId");

                    b.HasIndex("SenderId");

                    b.ToTable("TaskChatMessages");
                });

            modelBuilder.Entity("old_planner_api.src.Domain.Models.TaskModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("BoardId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAtDate")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("CreatorId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("DraftOfTaskId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("HexColor")
                        .HasMaxLength(7)
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDraft")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PriorityOrder")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("StartDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("BoardId");

                    b.HasIndex("CreatorId");

                    b.HasIndex("DraftOfTaskId");

                    b.ToTable("Tasks");
                });

            modelBuilder.Entity("old_planner_api.src.Domain.Models.UserModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("Image")
                        .HasColumnType("TEXT");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("RestoreCode")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("RestoreCodeValidBefore")
                        .HasColumnType("TEXT");

                    b.Property<string>("RoleName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Token")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("TokenValidBefore")
                        .HasColumnType("TEXT");

                    b.Property<bool>("WasPasswordResetRequest")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("Token");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("old_planner_api.src.Domain.Models.BoardMember", b =>
                {
                    b.HasOne("old_planner_api.src.Domain.Models.Board", "Board")
                        .WithMany("Members")
                        .HasForeignKey("BoardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("old_planner_api.src.Domain.Models.UserModel", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Board");

                    b.Navigation("User");
                });

            modelBuilder.Entity("old_planner_api.src.Domain.Models.ChatMembership", b =>
                {
                    b.HasOne("old_planner_api.src.Domain.Models.Chat", "Chat")
                        .WithMany("ChatMemberships")
                        .HasForeignKey("ChatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("old_planner_api.src.Domain.Models.UserModel", "User")
                        .WithMany("ChatMemberships")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Chat");

                    b.Navigation("User");
                });

            modelBuilder.Entity("old_planner_api.src.Domain.Models.ChatMessage", b =>
                {
                    b.HasOne("old_planner_api.src.Domain.Models.Chat", "Chat")
                        .WithMany("Messages")
                        .HasForeignKey("ChatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("old_planner_api.src.Domain.Models.UserModel", "Sender")
                        .WithMany("SentMessages")
                        .HasForeignKey("SenderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Chat");

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("old_planner_api.src.Domain.Models.DeletedTask", b =>
                {
                    b.HasOne("old_planner_api.src.Domain.Models.TaskModel", "Task")
                        .WithOne("DeletedTask")
                        .HasForeignKey("old_planner_api.src.Domain.Models.DeletedTask", "TaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Task");
                });

            modelBuilder.Entity("old_planner_api.src.Domain.Models.TaskChat", b =>
                {
                    b.HasOne("old_planner_api.src.Domain.Models.TaskModel", "Task")
                        .WithOne("Chat")
                        .HasForeignKey("old_planner_api.src.Domain.Models.TaskChat", "TaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Task");
                });

            modelBuilder.Entity("old_planner_api.src.Domain.Models.TaskChatMembership", b =>
                {
                    b.HasOne("old_planner_api.src.Domain.Models.TaskChat", "Chat")
                        .WithMany("Memberships")
                        .HasForeignKey("ChatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("old_planner_api.src.Domain.Models.UserModel", "Participant")
                        .WithMany("TaskChatMemberships")
                        .HasForeignKey("ParticipantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Chat");

                    b.Navigation("Participant");
                });

            modelBuilder.Entity("old_planner_api.src.Domain.Models.TaskChatMessage", b =>
                {
                    b.HasOne("old_planner_api.src.Domain.Models.TaskChat", "Chat")
                        .WithMany("Messages")
                        .HasForeignKey("ChatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("old_planner_api.src.Domain.Models.UserModel", "Sender")
                        .WithMany("ChatMessages")
                        .HasForeignKey("SenderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Chat");

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("old_planner_api.src.Domain.Models.TaskModel", b =>
                {
                    b.HasOne("old_planner_api.src.Domain.Models.Board", "Board")
                        .WithMany("Tasks")
                        .HasForeignKey("BoardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("old_planner_api.src.Domain.Models.UserModel", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("old_planner_api.src.Domain.Models.TaskModel", "DraftOfTask")
                        .WithMany()
                        .HasForeignKey("DraftOfTaskId");

                    b.Navigation("Board");

                    b.Navigation("Creator");

                    b.Navigation("DraftOfTask");
                });

            modelBuilder.Entity("old_planner_api.src.Domain.Models.Board", b =>
                {
                    b.Navigation("Members");

                    b.Navigation("Tasks");
                });

            modelBuilder.Entity("old_planner_api.src.Domain.Models.Chat", b =>
                {
                    b.Navigation("ChatMemberships");

                    b.Navigation("Messages");
                });

            modelBuilder.Entity("old_planner_api.src.Domain.Models.TaskChat", b =>
                {
                    b.Navigation("Memberships");

                    b.Navigation("Messages");
                });

            modelBuilder.Entity("old_planner_api.src.Domain.Models.TaskModel", b =>
                {
                    b.Navigation("Chat")
                        .IsRequired();

                    b.Navigation("DeletedTask");
                });

            modelBuilder.Entity("old_planner_api.src.Domain.Models.UserModel", b =>
                {
                    b.Navigation("ChatMemberships");

                    b.Navigation("ChatMessages");

                    b.Navigation("SentMessages");

                    b.Navigation("TaskChatMemberships");
                });
#pragma warning restore 612, 618
        }
    }
}
